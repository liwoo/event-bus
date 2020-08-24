# Event Bus
This is an example of how you can use **topic-based** messaging in your Microservices using [RabbitMQ]("https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html") in [.NET Core]("https://docs.microsoft.com/en-us/aspnet/core/getting-started/?view=aspnetcore-3.1&tabs=linux").

You will find 3 [Worker Services]("https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio") called **Basket**, **Order** and **Shipping**, in the following topology:

![alt Topology]("./topology.png")

In this case, the Basket publishes to a topic events that could be _created_, _updated_ or _deleted_.

The Order Service consumes any such event from the Basket and the Shipping Service only consumes the created events from the Basket.

The key is to agree on the Routing and Binding Keys as documented in the docs folder.

# Setup

``` bash
git clone [repo]
cd event-bus
dotnet restore
dotnet user-secrets set "RabbitMQ:Username" [yourUsername] --project "ShoppingBasket/" #set RabbitMQ Username on specific Project
dotnet user-secrets set "RabbitMQ:Password" [yourUsername] --project "ShoppingBasket/"
#do this for all three Projects
#don't forget to check your appsettings.json to make sure your topic and RabbitMQ Host is setup correctly
#you can change the 
mv .env.example .env #create an .env file to add the RABBIT_USERNAME and RABBIT_PASSWORD 
docker-compose up #make sure you have Docker Running in your machine
#in three seperate terminals (you can split your terminal)
dotnet run --project ShoppingBasket/ShoppingBasket.csproj
#do this for all three Projects
#log in to your RabbitMQ Admin and look for your exchange.
#you can even add messages to the routing key of your choice (basket.created) with a payload from the Admin and watch the consumers pick it up!
```