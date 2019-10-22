Repository for client tools and demos

# Prerequisites
* NodeJS / NPM - https://nodejs.org/en/

# Instructions
```
cd browser-client
npm install
./serve.sh
```

## Tools & Demos

### Map
URL: http://localhost:8080/map.html

Shows a map that is published to `/mapstate` topic with [nav_msgs/OccupancyGrid](http://docs.ros.org/api/nav_msgs/html/msg/OccupancyGrid.html) information.

Press the button in the UI to load (actually publish) a dummy 2-by-2 map.
