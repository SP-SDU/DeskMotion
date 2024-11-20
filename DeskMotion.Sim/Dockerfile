FROM python:3.12-slim

WORKDIR /app
COPY DeskMotion.Sim/ /app
EXPOSE 8000

ENTRYPOINT ["sh", "-c", "python -u simulator/main.py ${args}"]
