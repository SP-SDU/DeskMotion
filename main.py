import argparse
import ssl
from http.server import HTTPServer
from desk_manager import DeskManager
from simple_rest_server import SimpleRESTServer

def run(server_class=HTTPServer, handler_class=SimpleRESTServer, port=8000, use_https=False, cert_file=None, key_file=None):
    desk_manager = DeskManager()
    desk_manager.start_updates()

    def handler(*args, **kwargs):
        handler_class(desk_manager, *args, **kwargs)

    server_address = ("localhost", port)
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

    print(f"Starting {protocol} server on port {port}...")
    
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
    parser.add_argument("--port", type=int, default=8000, help="Port to run the server on (default: 8000)")
    parser.add_argument("--https", action="store_true", help="Enable HTTPS")
    parser.add_argument("--certfile", type=str, help="Path to the SSL certificate file")
    parser.add_argument("--keyfile", type=str, help="Path to the SSL key file")

    args = parser.parse_args()

    run(
        port=args.port,
        use_https=args.https,
        cert_file=args.certfile,
        key_file=args.keyfile
    )
