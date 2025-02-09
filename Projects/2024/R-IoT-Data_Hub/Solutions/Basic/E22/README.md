# E22 LoRa modem by EBYTE
- Issue: Two modems not communicating together. Solution: Make them same in channel, NET, airrate and other parameters, except address.
- Issue: Data looks like to be receved even after the sender is turned off. Solution: The modem has a 4096B big buffer. If not read yet, the attemp to read from the console will read the old data. The receiver stored this data in the buffer and when requested presents these even after the sender is off.
- Issue: Not being able to set or get the current config. Solution: The M1 needs to be connected to Vcc nad the M0 pin to GND to make the modem work in configuration mode. In standard Send/RCV mode both pins need to be grounded.  
