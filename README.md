## Overview

Multiplex Transport allows developer to combine many transports to run on the server at once.  

This is a good use case if you want to connect using UDP for your desktop or mobile, and use WebSocket for WebGL.

Please note that Transport A and Transport B cannot listen to the same port. The 2nd protocol or more port will be determined by `PortOffsetPerTransport`

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