
# UrlShortener

This project is a URL shortener web API built using ASP.NET Core 8. It generates short URLs using base62 encoding and stores them in MongoDB. The project also integrates with RabbitMQ for logging and tracking access counts of shortened URLs.


## Tech Stack

**Server:** ASP.NET Core 8, MongoDB, RabbitMQ, Docker


## Features

- Shorten long URLs using base62 encoding.
- Store shortened URLs and their mappings in MongoDB.
- Log URL access and increment access count via RabbitMQ.
- RabbitMQ consumers work as a background service.
- Supports Docker and runs in a containerized environment.

