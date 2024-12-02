FROM python:3.12-slim

RUN apt-get update && apt-get install -y git

WORKDIR /app

RUN git clone https://source.coderefinery.org/sdm-edu/wifi2ble-box-simulator.git .

RUN sed -i 's/localhost/0.0.0.0/' simulator/main.py

EXPOSE 8000

ENTRYPOINT ["sh", "-c", "python -u simulator/main.py"]
