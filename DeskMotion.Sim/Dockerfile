FROM python:3.12-slim

WORKDIR /app
COPY . /app
EXPOSE 8443

CMD ["sh", "-c", "python -u main.py --port 8443 --https --certfile cert.pem --keyfile key.pem"]
