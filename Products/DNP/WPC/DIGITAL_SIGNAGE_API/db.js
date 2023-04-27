// Use the MariaDB Node.js Connector
var mariadb = require('mariadb');
require('dotenv').config();
 
// Create a connection pool
var pool = mariadb.createPool({
    host: process.env.DB_SMART_SHELF_HOST, 
    port: process.env.DB_SMART_SHELF_PORT,
    user: process.env.DB_SMART_SHELF_USER, 
    password: process.env.DB_SMART_SHELF_PASSWORD,
    database: process.env.DB_SMART_SHELF_NAME,
  });

// Create a connection pool
var poolPcStore = mariadb.createPool({
  host: process.env.DB_PC_STORE_HOST, 
  port: process.env.DB_PC_STORE_PORT,
  user: process.env.DB_PC_STORE_USER, 
  password: process.env.DB_PC_STORE_PASSWORD,
  database: process.env.DB_PC_STORE_NAME,
});
 
// Expose a method to establish connection with MariaDB
module.exports = Object.freeze({
  pool: pool,
  poolPcStore: poolPcStore,
});