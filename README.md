## Features

It monitors the OpenVPN logfile. When someone logs in or out it will (optionally) send an push notification.
It also logs all attempts including IP and Geolocation info in .json and .txt format. The later can be used to import in firewall application to block these IP's in the future.

It has been tested on Synology NAS with DSM 6.

## Installation

1. Choose right build for your OS and unpack.  
2. Copy the `settings.default.json` to `settings.json`  
3. If you want push notifications when a user logs in with VPN set `"PushOver"` > `"Enabled": true`.  
   Log in to https://pushover.net/. First `create an application` and fill in all necessary details.  
   Copy the `User Key` or `Group Key` to the `"PushOver"` settings.  
4. 