# Displaying data received from "air" station
This directory contains a Windows GUI app, that connects to an LoRa modem and displays its receved data. The code is for test purposes only and is free to share with no guarantee.  
Most of it was written by the DeepSeek AI chatbot due to limited amount of time and upcomming deadline. 
This tool has some inperfections, the known are:  
* baudrate and data format selections havo no labels, the user might not know what does it mean
* The selection of data interpretation format says: Hexadecimal bytes with all metadata, which is true only in the read log console. If the user wants to enter bytes to send, he actualy enters only the data, not metadata...
* changing the data interpretation format while reading looks ugly, because it does not convert the data read in the old format into the newly selected one
