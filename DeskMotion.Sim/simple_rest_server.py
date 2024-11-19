import json
from http.server import BaseHTTPRequestHandler, HTTPServer
from desk_manager import DeskManager

class SimpleRESTServer(BaseHTTPRequestHandler):
    VERSION = "v2"
    API_KEYS = ["E9Y2LxT4g1hQZ7aD8nR3mWx5P0qK6pV7", "F7H1vM3kQ5rW8zT9xG2pJ6nY4dL0aZ3K"]
    
    def __init__(self, desk_manager, *args, **kwargs):
        self.desk_manager = desk_manager  # Set the DeskManager instance
        self.path_parts = []
        self.API_KEYS = self.load_api_keys()  # Load API keys from file
        super().__init__(*args, **kwargs)

    @staticmethod
    def load_api_keys():
        try:
            with open("api_keys.json", "r") as f:
                return json.load(f)
        except (FileNotFoundError, json.JSONDecodeError) as e:
            print(f"Error loading API keys: {e}")
            return []
    
    def _send_response(self, status_code, data):
        self.send_response(status_code)
        self.send_header("Content-Type", "application/json")
        self.end_headers()
        self.wfile.write(json.dumps(data).encode("utf-8"))
    
    def _is_valid_path(self):
        # Path format: /api/<version>/<api_key>/desks[/<desk_id>]
        self.path_parts = self.path.strip("/").split("/")
    
        if len(self.path_parts) < 4 or self.path_parts[0] != "api":
            self._send_response(400, {"error": "Invalid endpoint"})
            return False
    
        version = self.path_parts[1]
        api_key = self.path_parts[2]
    
        if api_key not in self.API_KEYS:
            self._send_response(401, {"error": "Unauthorized"})
            return False
    
        if version != self.VERSION:
            self._send_response(400, {"error": "Invalid API version"})
            return False
        
        return True
    
    def do_GET(self):
        if not self._is_valid_path():
            return
        
        if self.path_parts[3] == "desks":
            if len(self.path_parts) == 4:
                desk_ids = self.desk_manager.get_desk_ids()
                self._send_response(200, desk_ids)
            elif len(self.path_parts) == 5:
                desk_id = self.path_parts[4]
                desk = self.desk_manager.get_desk_data(desk_id)
                if desk:
                    self._send_response(200, desk)
                else:
                    self._send_response(404, {"error": "Desk not found"})
            elif len(self.path_parts) == 6:
                desk_id = self.path_parts[4]
                category = self.path_parts[5]
                data = self.desk_manager.get_desk_category(desk_id, category)
                if data:
                    self._send_response(200, data)
                else:
                    self._send_response(404, {"error": "Category not found"})
            else:
                self._send_response(400, {"error": "Invalid path"})
        else:
            self._send_response(400, {"error": "Invalid endpoint"})
    
    def do_PUT(self):
        if not self._is_valid_path():
            return

        if self.path_parts[3] == "desks":
            if len(self.path_parts) == 6:
                # Update a specific category of a specific desk
                try:
                    content_length = int(self.headers["Content-Length"])
                    post_data = self.rfile.read(content_length)
                    update_data = json.loads(post_data)
                    desk_id = self.path_parts[4]
                    category = self.path_parts[5]
                    success = self.desk_manager.update_desk_category(desk_id, category, update_data)
                    if success:
                        current_target_position = self.desk_manager.get_desk(desk_id).get_target_position()
                        response_data = {
                            "position_mm": current_target_position
                        }
                        self._send_response(200, response_data)
                    else:
                        self._send_response(404, {"error": "Category not found or desk not found"})
                except ValueError:
                    self._send_response(400, {"error": "Invalid data"})
                except TypeError:
                    self._send_response(400, {"error": "Invalid type"})
            else:
                self._send_response(400, {"error": "Invalid path"})
        else:
            self._send_response(400, {"error": "Invalid endpoint"})

    def do_POST(self):
        """Handle unsupported POST method."""
        self._send_response(405, {"error": "Method Not Allowed"})
    
    def do_DELETE(self):
        """Handle unsupported DELETE method."""
        self._send_response(405, {"error": "Method Not Allowed"})
    
    def do_PATCH(self):
        """Handle unsupported PATCH method."""
        self._send_response(405, {"error": "Method Not Allowed"})
