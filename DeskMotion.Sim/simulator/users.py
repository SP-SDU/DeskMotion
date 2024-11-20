from enum import Enum

class UserType(Enum):
    """Enumeration for user types."""
    SEATED = "seated"
    STANDING = "standing"
    ACTIVE = "active"

class UserBehavior:
    """Base class for user behaviors."""
    def __init__(self, desk):
        self.desk = desk

    def simulate(self):
        """Simulate user behavior. Override in subclasses."""
        pass

class SeatedUser(UserBehavior):
    """User who always keeps the desk in a seated position."""
    def __init__(self, desk, preffered_position=0):
        super().__init__(desk)
        if preffered_position < desk.min_position or preffered_position > desk.max_position:
            preffered_position = desk.min_position
            
        self.preffered_position = preffered_position
    
    def simulate(self):
        self.desk.set_target_position(self.preffered_position)

class StandingUser(UserBehavior):
    """User who always keeps the desk in a standing position."""
    def __init__(self, desk, preffered_position=0):
        super().__init__(desk)
        if preffered_position < desk.min_position or preffered_position > desk.max_position:
            preffered_position = desk.max_position

        self.preffered_position = preffered_position

    def simulate(self):
        self.desk.set_target_position(self.preffered_position)

class ActiveUser(UserBehavior):
    CYCLE_COUNT = 10
    """User who moves between seated and standing positions a few times a day."""
    def __init__(self, desk, seated_position=0, standing_position=0):
        super().__init__(desk)
        if seated_position < desk.min_position or seated_position > desk.max_position:
            seated_position = desk.min_position        

        self.seated_position = seated_position

        if standing_position < desk.min_position or standing_position > desk.max_position:
            standing_position = desk.max_position

        self.standing_position = standing_position
        self.next_position = self.seated_position
        self.cycle_counter = 0

    def simulate(self):
        # Alternate between seated and standing positions
        self.cycle_counter = (self.cycle_counter + 1) % self.CYCLE_COUNT

        if self.cycle_counter == 0:
            self.next_position = (
                self.standing_position if self.desk.state["position_mm"] <= self.seated_position else self.seated_position
            )
            self.desk.set_target_position(self.next_position)
