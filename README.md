# RabbitMQ Projects Repository

## WaterMark Adding Project
- In this project, watermarking is applied to images uploaded from local files. Using the RabbitMQ message broker, the watermark process is transmitted to RabbitMQ queues and the user can continue his other work in the system without waiting. When the Subscriber communicates with the queue, the transaction takes place and the watermarked image is saved to the file in the system.

## Excel Create Project 
- In this project, it is aimed to transfer certain fields of a certain table in the database to an Excel table. Since the data in the table may be large in size, this process is transferred to the queue using RabbitMQ. When the Excel file is created, a real-time notification is sent to the user using the SignalR library.
