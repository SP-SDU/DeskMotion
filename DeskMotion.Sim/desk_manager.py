import threading
import time
import json
import os
from desk import Desk

class DeskManager:
    STATE_FILE = "desks_state.json"
    
    def __init__(self):
        # Initialize desks with unique IDs and basic config
        self.desks = {
            "cd:fb:1a:53:fb:e6": Desk("cd:fb:1a:53:fb:e6", "DESK 4486", "Linak A/S"),
            "ee:62:5b:b8:73:1d": Desk("ee:62:5b:b8:73:1d", "DESK 6743", "Linak A/S"),
        }
        self.update_thread = None
        self.stop_event = threading.Event()
        self.lock = threading.Lock()
        self.load_state()

    def get_desk_ids(self):
        """Return the list of desk IDs."""
        with self.lock:
            return list(self.desks.keys())

    def get_desk(self, desk_id):
        """Get a desk by its ID."""
        with self.lock:
            desk = self.desks.get(desk_id)
            return desk if desk else None

    def get_desk_data(self, desk_id):
        """Get a desk's data snapshot by its ID."""
        with self.lock:
            desk = self.desks.get(desk_id)
            return desk.get_data() if desk else None

    def get_desk_category(self, desk_id, category):
        """Get a specific category from a desk."""
        with self.lock:
            desk = self.desks.get(desk_id)
            if desk:
                data = desk.get_data()
                return data.get(category)
            return None

    def update_desk_category(self, desk_id, category, data):
        """Update a specific category of a desk."""
        with self.lock:
            desk = self.desks.get(desk_id)
            return desk.update_category(category, data) if desk else False

    def _update_all_desks(self):
        """Continuously update each desk's position."""
        while not self.stop_event.is_set():
            with self.lock:
                for desk in self.desks.values():
                    desk.update_position()
            time.sleep(1)

    def start_updates(self):
        """Start the update thread for dynamic desk state changes."""
        if self.update_thread is None or not self.update_thread.is_alive():
            self.stop_event.clear()
            self.update_thread = threading.Thread(target=self._update_all_desks)
            self.update_thread.start()

    def stop_updates(self):
        """Stop the update thread."""
        if self.update_thread:
            self.stop_event.set()
            self.update_thread.join()
        self.save_state()

    def save_state(self):
        """Save the current state of all desks to a JSON file."""
        with open(self.STATE_FILE, "w") as f:
            json.dump({desk_id: desk.get_data() for desk_id, desk in self.desks.items()}, f)
        print(f"Desk state saved to {self.STATE_FILE}")

    def load_state(self):
        """Load the state of desks from a JSON file, if it exists."""
        if os.path.exists(self.STATE_FILE):
            with open(self.STATE_FILE, "r") as f:
                try:
                    data = json.load(f)
                    for desk_id, desk_data in data.items():
                        self.desks[desk_id] = Desk(desk_id, desk_data["config"]["name"], desk_data["config"]["manufacturer"], desk_data["state"]["position_mm"])
                        desk = self.desks[desk_id]
                        desk.config.update(desk_data["config"])
                        desk.state.update(desk_data["state"])
                        desk.usage.update(desk_data["usage"])
                        desk.lastErrors = desk_data["lastErrors"]
                    print(f"Desk state loaded from {self.STATE_FILE}")
                except json.JSONDecodeError:
                    print(f"Failed to decode {self.STATE_FILE}. Starting with default state.")
        else:
            print("No previous state found. Starting with default desk states.")
