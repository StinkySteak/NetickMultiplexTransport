## Overview

Multiplex Transport allows developer to combine many transports to run on the server at once.  

This is a good use case if you want to connect using UDP for your desktop or mobile, and use WebSocket for WebGL.

Please note that Transport A and Transport B cannot listen to the same port. The 2nd protocol or more port will be determined by `PortOffsetPerTransport`

| Feature   	| Status 	|
|-----------	|--------	|
| Multiplex 	| Beta   	|

## Installation

### Prerequisites

Unity Editor version 2021 or later.

Install Netick 2 before installing this package.
https://github.com/NetickNetworking/NetickForUnity

### Steps

- Open the Unity Package Manager by navigating to Window > Package Manager along the top bar.
- Click the plus icon.
- Select Add package from git URL
- Enter https://github.com/StinkySteak/NetickMultiplexTransport.git
- You can then create an instance by by double clicking in the Assets folder and going to `Create > Netick > Transport > MultiplexTransportProvider`

### How to Use?
The MultiplexTransport has several parameters

#### Server
- Populate the choice of your transport to the Multiplex
- Port Offset will be used to set the 2nd, or 3rd transport to avoid socket collision

#### Client
There are 2 available ways to connect to a server that is running on a multiplex transport.

|                      	| Manual                                                       	| Auto                                                                                      	|
|----------------------	|--------------------------------------------------------------	|-------------------------------------------------------------------------------------------	|
| Transport            	| Corresponding Transport (e.g LiteNetLib)                     	| using Multiplex                                                                           	|
| Port Number Choosing 	| Required to figure out matching port as the client transport 	| Auto however, can be mismatched, if server and client build multiplex config is different 	|