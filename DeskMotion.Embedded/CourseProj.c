#include <stdio.h>
#include <string.h>
#include "pico/stdlib.h"
#include "pico/cyw43_arch.h"
#include "pico/multicore.h"
#include "lwip/tcp.h"

const char *wifiName = "something"
const char *Password = "something else"

const uint buz_pin = 20;
const uint led_pin = 7;

const uint8_t ledMult = 5; //max led 25 timing 25 sec due to uint8_t of buzzerMultiplier
const uint8_t buzMult = ledMult * 10;

//led control
void led_run(uint8_t max)
{
    uint8_t i = 0;

    while (i < max)
    {
        gpio_put(led_pin, true);
        sleep_ms(500);
        gpio_put(led_pin, false);
        sleep_ms(500);
        i++;
    }
}
//buz control
void buz_run(uint8_t max)
{   
    uint8_t i = 0;

    while (i < max)
    {
        gpio_put(buz_pin, true);
        sleep_ms(90);
        gpio_put(buz_pin, false);
        sleep_ms(10);
        i++;
    }
}

// Entry function for the second core
void core1_entry() {
    printf("2nd thread is up\n");

    while (true) {
        uint32_t command = multicore_fifo_pop_blocking(); // Wait for a command from Core 0
        printf("Core 1 received command: %u\n", command);

        // Handle the command (e.g., run the buzzer)
        if (command == 1) {
            buz_run(buzMult); // Execute the buzzer task
        }

        // Signal back to Core 0 that the task is complete
        multicore_fifo_push_blocking(0);
    }
}

// Callback for incoming TCP data
static err_t http_server_recv(void *arg, struct tcp_pcb *tpcb, struct pbuf *p, err_t err) {
    if (!p) {
        printf("Client disconnected.\n");
        tcp_close(tpcb);
        return ERR_OK;
    }

    char *data = (char *)p->payload;
    printf("Received request:\n%s\n", data);

    //second core starts working
    printf("Sending command to Core 1...\n");
    multicore_fifo_push_blocking(1); // run buzzer


    led_run(ledMult);

    // wait core 1
    multicore_fifo_pop_blocking();
    printf("Core 1 task completed.\n");

    // Prepare HTTP response
    const char *response =
        "HTTP/1.1 200 OK\r\n"
        "Content-Type: text/plain\r\n"
        "Content-Length: 13\r\n"
        "Connection: close\r\n"
        "\r\n"
        "Hello, World!";


    tcp_write(tpcb, response, strlen(response), TCP_WRITE_FLAG_COPY);
    tcp_output(tpcb);

    tcp_recved(tpcb, p->tot_len);
    pbuf_free(p);
    tcp_close(tpcb);

    printf("Connection closed.\n");
    return ERR_OK;
}

// Callback for accepting new connections
static err_t http_server_accept(void *arg, struct tcp_pcb *newpcb, err_t err) {
    printf("new connection accept\n");
    tcp_recv(newpcb, http_server_recv);
    return ERR_OK;
}

//main loop, requests
int main()
{
    stdio_init_all();

    gpio_init(led_pin);
    gpio_set_dir(led_pin, GPIO_OUT);

    gpio_init(buz_pin);
    gpio_set_dir(buz_pin, GPIO_OUT);

    // Initialise the Wi-Fi chip
    if (cyw43_arch_init()) {
        printf("Wi-Fi init failed\n");
        return -1;
    }

    // Enable wifi station
    cyw43_arch_enable_sta_mode();

    printf("Connecting to Wi-Fi...\n");
    if (cyw43_arch_wifi_connect_timeout_ms(wifiName, Password, CYW43_AUTH_WPA2_AES_PSK, 30000)) {
        printf("failed to connect.\n");
        return 1;
    } else {
        printf("Connected.\n");
        // Read the ip address in a human readable way
        uint8_t *ip_address = (uint8_t*)&(cyw43_state.netif[0].ip_addr.addr);
        printf("IP address %d.%d.%d.%d\n", ip_address[0], ip_address[1], ip_address[2], ip_address[3]);
    }

    //debug (run outside)
    multicore_launch_core1(core1_entry);

    //tcp server
    struct tcp_pcb *server_pcb = tcp_new();
    tcp_bind(server_pcb, IP_ADDR_ANY, 80);
    server_pcb = tcp_listen(server_pcb);
    tcp_accept(server_pcb, http_server_accept);

    printf("HTTP server running on port 80\n");


    while (true) 
    {
        cyw43_arch_poll();//listen to wifi
    }

    cyw43_arch_deinit();
    return 0;
}
