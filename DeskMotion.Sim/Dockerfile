FROM python:latest

WORKDIR /app
COPY . /app
EXPOSE 8443

CMD ["sh", "-c", "python main.py --port 8443 --https --certfile cert.pem --keyfile key.pem"]
