# Simple Chat App in C\#

## Functionalities

### Implemented functionalities

- Account creation
- Login
- Create new topic
- See all topics
- Join a topic (only one at a time)
- Send a message to all users in the current topic
- Exit a topic
- See all users
- Enter a private discussion with any user
- Send a private message to any user
- Exit a private discussion
- Logout
- Save and retrieve the users, the topics and the lists of messages in files
- A safety when the client is closed to make the user exit either the topic or the private discussion, log out and to terminate the corresponding thread in the server

### Unimplemented functionalities

- Graphical interface

### Possible improvements

- Print a notification when a private message is received

## Implementation

### Chat App project

It is the project that serves as the server.  
It contains all the methods to handle all the types of requests that come from the client(s) and to send them responses.
It also contains the FileManager class that is used to write to and retrieve information from files.

### Client project

It is the project that serves as the client(s). It can be launched multiple times to open multiple clients.  
It contains all the methods for a user to use all the functionalities all the app.
It also sends requests to the server and receive responses from it.

### Communication project

This project allows the communication between the server and the client(s).  
It contains the definition of all request and response type classes.
It also contains method to serialize and deserialize requests and responses.  
This project is in the dependencies of the Server and Client projects.

### Models project

This project contains the definition of all the custom Types that are used within the app.  
All the other 3 projects have this one in their dependencies.