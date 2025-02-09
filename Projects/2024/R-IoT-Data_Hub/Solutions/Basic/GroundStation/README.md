# Displaying data received from "air" station
  This directory contains a Windows GUI app, that connects to an LoRa modem and displays its receved data. The code is for test purposes only and is free to share with no guarantee.  
  ![Graph view of the tool](https://github.com/user-attachments/assets/48335cf6-d814-4ec7-8ad2-7138caabb31f)

  It reads from a selected COM port at a specified baudrate, writes to the console log in full HEXadecimal format or a data-only text interpreted format. 
  Saving the log to a text file is supported, in the future hopefully also to an excell sheet. 
  The user can switch to an graph view and see a live update of the data visually. However only a format of T:\<number>, H:\<number> is supported and should work.  
  Sending to a broadcas addres is currently implemented, both devices need to have the same params like airrate, channel etc. to communicate.  
  Most of it was written by the DeepSeek AI chatbot due to limited amount of time and upcomming deadline. 
This tool has some inperfections, the known are:  
* baudrate and data format selections havo no labels, the user might not know what does it mean
* The selection of data interpretation format says: Hexadecimal bytes with all metadata, which is true only in the read log console. If the user wants to enter bytes to send, he actualy enters only the data, not metadata...
* changing the data interpretation format while reading looks ugly, because it does not convert the data read in the old format into the newly selected one
* the graph view only suports drawing temp. and hum. data in this following format: T:\<number>, H:\<number>. It is not redundant to malformed data.
