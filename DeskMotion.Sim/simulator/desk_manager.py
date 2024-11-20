import threading
import time
import json
import os
import random
from desk import Desk
from users import UserBehavior, SeatedUser, StandingUser, ActiveUser, UserType

class DeskManager:
    STATE_FILE = "data/desks_state.json"
    SECONDS_PER_DAY = 86400
    DAY_START_HOUR = 6
    NIGHT_START_HOUR = 18
    INCREMENT_SECS = 60
    POWER_OFF_CHANCE = 0.05
    
    def __init__(self):
        self.desks = {}
        self.users = {}
        self.powered_off_desks = {}
        self.update_thread = None
        self.simulation_thread = None
        self.power_off_thread = None
        self.stop_event = threading.Event()
        self.lock = threading.Lock()
        self.current_time_s = 43200
        self.load_state()

    def get_desk_ids(self):
        """Return the list of desk IDs, excluding powered-off desks."""
        with self.lock:
            return [desk_id for desk_id in self.desks if desk_id not in self.powered_off_desks]

    def get_desk(self, desk_id):
        """Get a desk by its ID."""
        with self.lock:
            if desk_id in self.powered_off_desks:
                return None
            return self.desks.get(desk_id)

    def get_desk_data(self, desk_id):
        """Get a desk's data snapshot by its ID."""
        desk = self.get_desk(desk_id)
        return desk.get_data() if desk else None

    def get_desk_category(self, desk_id, category):
        """Get a specific category from a desk."""
        desk = self.get_desk(desk_id)
        if desk:
            data = desk.get_data()
            return data.get(category)
        return None

    def update_desk_category(self, desk_id, category, data):
        """Update a specific category of a desk."""
        desk = self.get_desk(desk_id)
        return desk.update_category(category, data) if desk else False

    def add_desk(self, desk_id, name, manufacturer, user_type: UserType):
        """Add a new desk with a unique ID."""
        with self.lock:
            if desk_id not in self.desks:
                desk = Desk(desk_id, name, manufacturer)
                self.desks[desk_id] = desk
                self.users[desk_id] = self._create_user(desk, user_type)
                return True
            print(f"Desk with ID {desk_id} already exists.")
            return False

    def remove_desk(self, desk_id):
        """Remove a desk by its ID."""
        with self.lock:
            if desk_id in self.desks:
                del self.desks[desk_id]
                del self.users[desk_id]
                self.powered_off_desks.pop(desk_id, None)
                return True
            print(f"Desk with ID {desk_id} does not exist.")
            return False
            
    def is_daytime(self):
        """Check if the current time is during the day."""
        simulated_time = (self.current_time_s % self.SECONDS_PER_DAY) / 3600
        return self.DAY_START_HOUR <= simulated_time < self.NIGHT_START_HOUR

    def increment_time(self):
        """Increment the simulation time by one second."""
        with self.lock:
            self.current_time_s += self.INCREMENT_SECS
            if self.current_time_s >= self.SECONDS_PER_DAY:
                self.current_time_s = 0

    def _create_user(self, desk, user_type: UserType):
        """Create a behavior instance based on the behavior type."""
        if user_type == UserType.SEATED:
            return SeatedUser(desk)
        elif user_type == UserType.STANDING:
            return StandingUser(desk)
        elif user_type == UserType.ACTIVE:
            return ActiveUser(desk)
        else:
            raise ValueError(f"Unknown behavior type: {user_type}")
                            
    def _update_all_desks(self):
        """Continuously update each desk's position."""
        while not self.stop_event.is_set():
            with self.lock:
                for desk_id, desk in self.desks.items():
                    if desk_id not in self.powered_off_desks:
                        desk.update()
            time.sleep(1)
            self.increment_time()

    def _simulate_user_interactions(self):
        """Simulate local user interactions for all desks."""
        while not self.stop_event.is_set():
            if self.is_daytime():
                with self.lock:
                    for desk_id, user in self.users.items():
                        if desk_id not in self.powered_off_desks:
                            user.simulate()
            time.sleep(5)

    def _simulate_power_off(self):
        """Randomly power off desks for a period of time."""
        while not self.stop_event.is_set():
            with self.lock:
                if self.desks and random.random() < self.POWER_OFF_CHANCE:
                    desk_id = random.choice(list(self.desks.keys()))
                    if desk_id not in self.powered_off_desks:
                        power_off_duration = random.randint(5*60, 2*60*60)
                        self.powered_off_desks[desk_id] = self.current_time_s + power_off_duration
            time.sleep(5)

            with self.lock:
                desks_to_restore = [
                    desk_id for desk_id, power_on_time in self.powered_off_desks.items()
                    if self.current_time_s >= power_on_time
                ]
                for desk_id in desks_to_restore:
                    del self.powered_off_desks[desk_id]

    def start_updates(self):
        """Start the update and simulation threads."""
        if self.update_thread is None or not self.update_thread.is_alive():
            self.stop_event.clear()
            self.update_thread = threading.Thread(target=self._update_all_desks)
            self.update_thread.start()

        if self.simulation_thread is None or not self.simulation_thread.is_alive():
            self.simulation_thread = threading.Thread(target=self._simulate_user_interactions)
            self.simulation_thread.start()

        if self.power_off_thread is None or not self.power_off_thread.is_alive():
            self.power_off_thread = threading.Thread(target=self._simulate_power_off)
            self.power_off_thread.start()

    def stop_updates(self):
        """Stop the update and simulation threads."""
        if self.update_thread or self.simulation_thread:
            self.stop_event.set()
            if self.update_thread:
                self.update_thread.join()
            if self.simulation_thread:
                self.simulation_thread.join()
            if self.power_off_thread:
                self.power_off_thread.join()
        self.save_state()

    def save_state(self):
        """Save the current state of desks and users."""
        state = {}
        with self.lock:
            for desk_id, desk in self.desks.items():
                state[desk_id] = {
                    "desk_data": desk.get_data(),
                    "user": self.users[desk_id].__class__.__name__.replace("User", "").lower(),
                }
                state[desk_id]["desk_data"]["clock_s"] = desk.clock_s
                state["current_time_s"] = self.current_time_s
        with open(self.STATE_FILE, "w") as f:
            json.dump(state, f)
        print(f"Desk Manager state saved to {self.STATE_FILE}")

    def load_state(self):
        """Load the state of desks and users from a JSON file, if it exists."""
        if os.path.exists(self.STATE_FILE):
            with open(self.STATE_FILE, "r") as f:
                try:
                    data = json.load(f)
                    self.current_time_s = data.get("current_time_s", 43200)
                    for desk_id, saved_data in data.items():
                        if desk_id == "current_time_s":
                            continue
                        desk_data = saved_data["desk_data"]
                        user_type = UserType(saved_data["user"])
                        desk = Desk(
                            desk_id,
                            desk_data["config"]["name"],
                            desk_data["config"]["manufacturer"],
                            desk_data["state"]["position_mm"],
                        )
                        desk.config.update(desk_data["config"])
                        desk.state.update(desk_data["state"])
                        desk.usage.update(desk_data["usage"])
                        desk.lastErrors = desk_data["lastErrors"]
                        desk.clock_s = desk_data["clock_s"]
                        self.desks[desk_id] = desk
                        self.users[desk_id] = self._create_user(desk, user_type)
                    print(f"Desk Manager state loaded from {self.STATE_FILE}")
                except (json.JSONDecodeError, KeyError, ValueError) as e:
                    print(f"Failed to load state from {self.STATE_FILE}: {e}. Starting with default state.")
        else:
            print("No previous state found. Starting with default desk states.")
