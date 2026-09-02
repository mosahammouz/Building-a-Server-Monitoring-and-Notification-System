# **Building a Server Monitoring and Notification System**

## **Task 1: Server Statistics Collection Service**

- **Objective:** Develop a C# process that collects and publishes server statistics, specifically memory usage, available memory, and CPU usage.
- **Requirements:**
    - Collect server statistics (memory usage, available memory, and CPU usage) at regular intervals, specified by a configurable period `SamplingIntervalSeconds`, using the `System.Diagnostics` namespace.
    - Encapsulate the collected statistics (memory usage, available memory, CPU usage, and timestamp) into a statistics object.
    - Publish the statistics object to a message queue under the topic `ServerStatistics.<ServerIdentifier>`, where `<ServerIdentifier>` is a configurable option.
    - Implement an abstraction for message queuing to ensure the code is not tightly coupled to RabbitMQ.

!server-monitoring-1.png

**Code Snippet: Server Statistics Data Type (Class Definition)**

```csharp
// Define a class for server statistics
public class ServerStatistics
{
    public double MemoryUsage { get; set; } // in MB
    public double AvailableMemory { get; set; } // in MB
    public double CpuUsage { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**Code Snippet: Server Statistics Configuration (`appsettings.json`)**

```json
// Configuration options in appsettings.json
{
    "ServerStatisticsConfig": {
        "SamplingIntervalSeconds": 60,
        "ServerIdentifier": "linux1"
    }
}
```

## **Task 2: Message Processing and Anomaly Detection Service**

- **Objective:** Develop a C# process that receives server statistics from the message queue, persists the data to a MongoDB instance, and sends alerts if anomalies or high usage is detected.
- **Requirements:**
    - Receive messages from the message queue with the topic `ServerStatistics.*` and deserialize them into objects containing memory usage, available memory, CPU usage, server's identifier, and timestamp.
    - Persist the deserialized data to a MongoDB instance.
    - Implement anomaly detection logic based on configurable threshold percentages specified in `appsettings.json`.
    - Send an "Anomaly Alert" via SignalR if a sudden increase (anomaly) in memory usage or CPU usage is detected.
    - Send a "High Usage Alert" via SignalR if the calculated memory usage percentage or CPU usage exceeds the specified "Usage Threshold".
    - Implement abstractions for MongoDB and message queuing to ensure the code is not tightly coupled to specific implementations.

!server-monitoring-2.png

**Equations for Alert Calculation:**

- For Anomaly Alert:
    - Memory Usage Anomaly Alert: `if (CurrentMemoryUsage > (PreviousMemoryUsage * (1 + MemoryUsageAnomalyThresholdPercentage)))`
    - CPU Usage Anomaly Alert: `if (CurrentCpuUsage > (PreviousCpuUsage * (1 + CpuUsageAnomalyThresholdPercentage)))`
- For High Usage Alert:
    - Memory High Usage Alert: `if ((CurrentMemoryUsage / (CurrentMemoryUsage + CurrentAvailableMemory)) > MemoryUsageThresholdPercentage)`
    - CPU High Usage Alert: `if (CurrentCpuUsage > CpuUsageThresholdPercentage)`

**Code Snippet: Server Statistics Data Type (Class Definition)**

```csharp
// Define a class for server statistics
public class ServerStatistics
{
    public String ServerIdentifier { get; set; }
    public double MemoryUsage { get; set; } // in MB
    public double AvailableMemory { get; set; } // in MB
    public double CpuUsage { get; set; }
		public DateTime Timestamp { get; set; }
}
```

**Code Snippet: Anomaly Detection Service Configuration (`appsettings.json`)**

```json
// Configuration options in appsettings.json
{
    "AnomalyDetectionConfig": {
        "MemoryUsageAnomalyThresholdPercentage": 0.4,
        "CpuUsageAnomalyThresholdPercentage": 0.5,
        "MemoryUsageThresholdPercentage": 0.8,
        "CpuUsageThresholdPercentage": 0.9
    },
    "SignalRConfig": {
        "SignalRUrl": "<http://your-signalr-hub-url>"
    }
}
```

## **Task 3: SignalR Event Consumer Service**

- **Objective:** Develop a C# process that connects to a SignalR hub and prints received events to the console.
- **Requirements:**
    - Establish a connection to a SignalR hub.
    - Subscribe to events and print them to the console as they are received.

!server-monitoring-3.png

**Code Snippet: SignalR Event Consumer Service Configuration (`appsettings.json`)**

```json
// Configuration options in appsettings.json
{
    "SignalRConfig": {
        "SignalRUrl": "<http://your-signalr-hub-url>"
    }
}
```

## **Optional Tasks:**

### **Optional Task 4: Create a Reusable RabbitMQ Client Library**

- **Objective:** Develop a reusable class library for interacting with RabbitMQ, thereby abstracting the RabbitMQ client and making it easier to switch to a different message queuing system in the future.
- **Requirements:**
    - Implement a class for publishing messages to a RabbitMQ exchange.
    - Implement a class for consuming messages from a RabbitMQ queue.
    - Both classes should utilize an interface, making the library extensible and not tightly coupled to RabbitMQ.

### **Optional Task 5: Dockerization of Services**

- **Objective:** Dockerize the services to enable containerized deployments.
- **Requirements:**
    - Modify the services to accept configuration options through environment variables instead of `appsettings.json`.
    - Ensure that all components (Server Statistics Collection Service, Message Processing and Anomaly Detection Service, and SignalR Event Consumer Service) are running and integrated correctly within Docker containers.

### **Optional Task 6: Docker Compose Integration**

- **Objective:** Use Docker Compose to orchestrate the deployment of the entire system.
- **Requirements:**
    - Create a Docker Compose file that defines the services, their dependencies, and their configurations.
    - Deploy the entire system, including RabbitMQ and MongoDB instances, using Docker Compose.

!server-monitoring-4.png

***Note:** It’s possible to create multiple versions of the same image using tags. Here, for example, we build two different versions of the image; one of them is intended to run on a Linux environment, and the other is intended to run on a Windows environment. We distinguish them with the image tag (i.e., `image:tag`).*

!server-monitoring-5.png