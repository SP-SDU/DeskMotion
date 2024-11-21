import argparse
import ssl
import random
from http.server import HTTPServer
from users import UserType
from desk_manager import DeskManager
from simple_rest_server import SimpleRESTServer

def generate_desk_id():
    return ":".join(f"{random.randint(0, 255):02x}" for _ in range(6))

def generate_desk_name():
    return f"DESK {random.randint(1000, 9999)}"

def run(server_class=HTTPServer, handler_class=SimpleRESTServer, host="localhost", port=8000, use_https=False, cert_file=None, key_file=None, desks=2):
    desk_manager = DeskManager()
    desk_manager.add_desk("cd:fb:1a:53:fb:e6", "DESK 4486", "Linak A/S", UserType.ACTIVE)
    desk_manager.add_desk("ee:62:5b:b8:73:1d", "DESK 6743", "Linak A/S", UserType.STANDING)

    if len(desk_manager.get_desk_ids()) < desks:
        for i in range(desks - len(desk_manager.get_desk_ids())):
            desk_manager.add_desk(generate_desk_id(), generate_desk_name(), "Linak A/S", UserType.ACTIVE)

    desk_manager.start_updates()

    def handler(*args, **kwargs):
        handler_class(desk_manager, *args, **kwargs)

    server_address = (host, port)
    httpd = server_class(server_address, handler)

    if use_https:
        if not cert_file or not key_file:
            raise ValueError("Both certificate and key files must be provided for HTTPS.")
        context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
        context.load_cert_chain(certfile=cert_file, keyfile=key_file)

        httpd.socket = context.wrap_socket(httpd.socket, server_side=True)
        protocol = "HTTPS"
    else:
        protocol = "HTTP"

    print(f"Now listening on: {protocol.lower()}://{host}:{port}")
    print("Application started. Press Ctrl+C to shut down.")

    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        desk_manager.stop_updates()  # Stop the desk updates
        print("Server stopped.")

"""
    To execute the script as HTTPS, use the following command:
        python main.py --port 8443 --https --certfile cert.pem --keyfile key.pem

    To execute the script as HTTP, use the following command:
        python main.py --port 8000
"""

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Start a simple REST API server.")
    parser.add_argument("--host", type=str, default="localhost", help="Host/IP to bind the server to (default: localhost)")
    parser.add_argument("--port", type=int, default=8000, help="Port to run the server on (default: 8000)")
    parser.add_argument("--https", action="store_true", help="Enable HTTPS")
    parser.add_argument("--certfile", type=str, help="Path to the SSL certificate file")
    parser.add_argument("--keyfile", type=str, help="Path to the SSL key file")
    parser.add_argument("--desks", type=int, default=2, help="Minimum number of desks to simulate (default: 2)")

    args = parser.parse_args()

    run(
        host=args.host,
        port=args.port,
        use_https=args.https,
        cert_file=args.certfile,
        key_file=args.keyfile,
        desks=args.desks
    )
