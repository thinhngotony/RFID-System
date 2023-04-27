const express = require('express');
const app = express();
const server = require('http').createServer(app);
require('dotenv').config();
const bodyParser = require("body-parser");
const db = require('./db');
const digitalSignageRoutes = require("./routes/digitalSignageRoutes");
var cors = require('cors')
app.use(cors())

var io = require('socket.io')(server, {
    cors: { origin: "*" }
}); 

io.on('connection', function (client) {
    console.log('SocketIO: New client connected...' + client.id);
    db.poolPcStore.query(`
        SELECT * FROM digital_display_config
        ORDER BY step  `,
    ).catch(error => {
        console.log(error);
        return res.send({ success: false, message: error });
    }).then(result => {
        if (result.length > 0) {
            io.emit("initData", result);
        }
    });
    io.on('disconnect', () => {
        console.log('SocketIO: Disconnected...' + client.id);
    });
});

app.use(function(req, res, next){
    req.io = io;
    next();
});

app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: false }));

app.use("/api", digitalSignageRoutes);

server.listen(process.env.SERVER_LISTEN_PORT, () => console.log(`Listening on port ${process.env.SERVER_LISTEN_PORT}`));