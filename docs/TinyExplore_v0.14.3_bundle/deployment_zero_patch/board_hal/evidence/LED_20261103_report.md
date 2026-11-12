# LED lifecycle test — 2026-11-03

Software command-path verification; physical light observation is not independently established. Pin and active polarity are read from the live open LED before mutation, not inferred from board defaults.

Completed AEP exchanges: 19; all ACKs confirmed.

Initial configuration: opened=True, pin=18, active=LOW. Test covers three BLINK calls, ON/OFF/TOGGLE, CLOSE, INFO while closed, ON while closed (expected ENOTOPEN), OPEN using captured 18/LOW, then repeat operational commands. Final state is open on original pin and polarity, OFF. No RKB filter configuration was changed.

```text
AT+PUB=SYS,LED,0x09037101,INFO
[14:44:58.413] [00000] AT Command: AT+PUB=SYS,LED,0x09037101,INFO
+DATA:09037101,SYS:led,opened=True,pin=18,active=LOW

```

```text
AT+PUB=SYS,LED,0x09037102,BLINK,2,100
[14:45:14.262] [00000] AT Command: AT+PUB=SYS,LED,0x09037102,BLINK,2,100
+OK:09037102,SYS:led,blink,count=2,phaseMs=100

```

```text
AT+PUB=SYS,LED,0x09037103,ON
[14:45:20.322] [00000] AT Command: AT+PUB=SYS,LED,0x09037103,ON
+OK:09037103,SYS:led,on

```

```text
AT+PUB=SYS,LED,0x09037104,OFF
[14:45:27.492] [00000] AT Command: AT+PUB=SYS,LED,0x09037104,OFF
+OK:09037104,SYS:led,off

```

```text
AT+PUB=SYS,LED,0x09037105,TOGGLE
[14:45:33.843] [00000] AT Command: AT+PUB=SYS,LED,0x09037105,TOGGLE
+OK:09037105,SYS:led,toggle

```

```text
AT+PUB=SYS,LED,0x09037106,TOGGLE
[14:45:42.234] [00000] AT Command: AT+PUB=SYS,LED,0x09037106,TOGGLE
+OK:09037106,SYS:led,toggle

```

```text
AT+PUB=SYS,LED,0x09037107,BLINK,3,100
[14:45:49.099] [00000] AT Command: AT+PUB=SYS,LED,0x09037107,BLINK,3,100
+OK:09037107,SYS:led,blink,count=3,phaseMs=100

```

```text
AT+PUB=SYS,LED,0x09037108,CLOSE
[14:45:55.852] [00000] AT Command: AT+PUB=SYS,LED,0x09037108,CLOSE
+OK:09037108,SYS:led,close

```

```text
AT+PUB=SYS,LED,0x09037109,INFO
[14:46:03.249] [00000] AT Command: AT+PUB=SYS,LED,0x09037109,INFO
+DATA:09037109,SYS:led,opened=False,pin=18,active=LOW

```

```text
AT+PUB=SYS,LED,0x0903710A,ON
[14:46:09.541] [00000] AT Command: AT+PUB=SYS,LED,0x0903710A,ON
+ERR:0903710A,SYS,ENOTOPEN

```

```text
AT+PUB=SYS,LED,0x0903710B,OPEN,18,LOW
[14:46:17.052] [00000] AT Command: AT+PUB=SYS,LED,0x0903710B,OPEN,18,LOW
+OK:0903710B,SYS:led,open

```

```text
AT+PUB=SYS,LED,0x0903710C,INFO
[14:46:24.832] [00000] AT Command: AT+PUB=SYS,LED,0x0903710C,INFO
+DATA:0903710C,SYS:led,opened=True,pin=18,active=LOW

```

```text
AT+PUB=SYS,LED,0x0903710D,BLINK,2,100
[14:46:30.804] [00000] AT Command: AT+PUB=SYS,LED,0x0903710D,BLINK,2,100
+OK:0903710D,SYS:led,blink,count=2,phaseMs=100

```

```text
AT+PUB=SYS,LED,0x0903710E,ON
[14:46:38.154] [00000] AT Command: AT+PUB=SYS,LED,0x0903710E,ON
+OK:0903710E,SYS:led,on

```

```text
AT+PUB=SYS,LED,0x0903710F,OFF
[14:46:45.822] [00000] AT Command: AT+PUB=SYS,LED,0x0903710F,OFF
+OK:0903710F,SYS:led,off

```

```text
AT+PUB=SYS,LED,0x09037110,TOGGLE
[14:46:51.664] [00000] AT Command: AT+PUB=SYS,LED,0x09037110,TOGGLE
+OK:09037110,SYS:led,toggle

```

```text
AT+PUB=SYS,LED,0x09037111,OFF
[14:46:58.421] [00000] AT Command: AT+PUB=SYS,LED,0x09037111,OFF
+OK:09037111,SYS:led,off

```

```text
AT+PUB=SYS,LED,0x09037112,INFO
[14:47:05.632] [00000] AT Command: AT+PUB=SYS,LED,0x09037112,INFO
+DATA:09037112,SYS:led,opened=True,pin=18,active=LOW

```

```text
/log?-80
+OK
[14:47:13.076] [00000] Command received: /log?-80, Method: GET
[14:45:49.118] [TRACE] SYS:publish_entry ctx=09037107 dest=SYS cmd=LED args=BLINK,3,100 sync=True fanout=False
[14:45:49.123] [TRACE] SYS:publish_match ctx=09037107 dest=SYS cmd=LED module=SYS
[14:45:49.128] [TRACE] SYS:route_entry ctx=09037107 cmd=LED args=BLINK,3,100
[14:45:49.773] [TRACE] LORA:QueueRX ctx=030006D2 count=0 From=9900 To=FFFF Len=14
[14:45:49.852] [TRACE] SYS:route_ok ctx=09037107 cmd=LED result=+OK:09037107,SYS:led,blink,count=3,phaseMs=100
[14:45:55.858] [00000] AT Command: AT+PUB=SYS,LED,0x09037108,CLOSE
[14:45:55.868] [TRACE] SYS:publish_entry ctx=09037108 dest=SYS cmd=LED args=CLOSE sync=True fanout=False
[14:45:55.872] [TRACE] SYS:publish_match ctx=09037108 dest=SYS cmd=LED module=SYS
[14:45:55.877] [TRACE] SYS:route_entry ctx=09037108 cmd=LED args=CLOSE
[14:45:55.883] [TRACE] SYS:route_ok ctx=09037108 cmd=LED result=+OK:09037108,SYS:led,close
[14:45:59.770] [TRACE] LORA:QueueRX ctx=030006D4 count=0 From=9900 To=FFFF Len=14
[14:46:02.743] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:46:03.257] [00000] AT Command: AT+PUB=SYS,LED,0x09037109,INFO
[14:46:03.268] [TRACE] SYS:publish_entry ctx=09037109 dest=SYS cmd=LED args=INFO sync=True fanout=False
[14:46:03.271] [TRACE] SYS:publish_match ctx=09037109 dest=SYS cmd=LED module=SYS
[14:46:03.875] [TRACE] SYS:route_entry ctx=09037109 cmd=LED args=INFO
[14:46:03.880] [TRACE] SYS:route_ok ctx=09037109 cmd=LED result=+DATA:09037109,SYS:led,opened=False,pin=18,active=LOW
[14:46:04.891] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:46:07.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:46:09.547] [00000] AT Command: AT+PUB=SYS,LED,0x0903710A,ON
[14:46:09.557] [TRACE] SYS:publish_entry ctx=0903710A dest=SYS cmd=LED args=ON sync=True fanout=False
[14:46:09.561] [TRACE] SYS:publish_match ctx=0903710A dest=SYS cmd=LED module=SYS
[14:46:09.565] [TRACE] SYS:route_entry ctx=0903710A cmd=LED args=ON
[14:46:09.570] [TRACE] SYS:route_ok ctx=0903710A cmd=LED result=+ERR:0903710A,SYS,ENOTOPEN
[14:46:09.697] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:46:17.060] [00000] AT Command: AT+PUB=SYS,LED,0x0903710B,OPEN,18,LOW
[14:46:17.071] [TRACE] SYS:publish_entry ctx=0903710B dest=SYS cmd=LED args=OPEN,18,LOW sync=True fanout=False
[14:46:17.075] [TRACE] SYS:publish_match ctx=0903710B dest=SYS cmd=LED module=SYS
[14:46:17.079] [TRACE] SYS:route_entry ctx=0903710B cmd=LED args=OPEN,18,LOW
[14:46:17.087] [TRACE] SYS:route_ok ctx=0903710B cmd=LED result=+OK:0903710B,SYS:led,open
[14:46:19.781] [TRACE] LORA:QueueRX ctx=030006D8 count=0 From=9900 To=FFFF Len=14
[14:46:23.911] [TRACE] LORA:QueueRX ctx=550000C7 count=0 From=5557 To=FFFF Len=11
[14:46:24.839] [00000] AT Command: AT+PUB=SYS,LED,0x0903710C,INFO
[14:46:24.850] [TRACE] SYS:publish_entry ctx=0903710C dest=SYS cmd=LED args=INFO sync=True fanout=False
[14:46:24.854] [TRACE] SYS:publish_match ctx=0903710C dest=SYS cmd=LED module=SYS
[14:46:24.858] [TRACE] SYS:route_entry ctx=0903710C cmd=LED args=INFO
[14:46:24.864] [TRACE] SYS:route_ok ctx=0903710C cmd=LED result=+DATA:0903710C,SYS:led,opened=True,pin=18,active=LOW
[14:46:28.052] [TRACE] LORA:QueueRX ctx=50000391 count=0 From=5000 To=FFFF Len=10
[14:46:29.781] [TRACE] LORA:QueueRX ctx=030006DA count=0 From=9900 To=FFFF Len=14
[14:46:30.810] [00000] AT Command: AT+PUB=SYS,LED,0x0903710D,BLINK,2,100
[14:46:30.822] [TRACE] SYS:publish_entry ctx=0903710D dest=SYS cmd=LED args=BLINK,2,100 sync=True fanout=False
[14:46:30.825] [TRACE] SYS:publish_match ctx=0903710D dest=SYS cmd=LED module=SYS
[14:46:30.830] [TRACE] SYS:route_entry ctx=0903710D cmd=LED args=BLINK,2,100
[14:46:31.232] [TRACE] SYS:route_ok ctx=0903710D cmd=LED result=+OK:0903710D,SYS:led,blink,count=2,phaseMs=100
[14:46:32.730] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:46:34.911] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:46:37.753] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:46:38.161] [00000] AT Command: AT+PUB=SYS,LED,0x0903710E,ON
[14:46:38.171] [TRACE] SYS:publish_entry ctx=0903710E dest=SYS cmd=LED args=ON sync=True fanout=False
[14:46:38.175] [TRACE] SYS:publish_match ctx=0903710E dest=SYS cmd=LED module=SYS
[14:46:38.179] [TRACE] SYS:route_entry ctx=0903710E cmd=LED args=ON
[14:46:38.185] [TRACE] SYS:route_ok ctx=0903710E cmd=LED result=+OK:0903710E,SYS:led,on
[14:46:39.522] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:46:45.828] [00000] AT Command: AT+PUB=SYS,LED,0x0903710F,OFF
[14:46:45.838] [TRACE] SYS:publish_entry ctx=0903710F dest=SYS cmd=LED args=OFF sync=True fanout=False
[14:46:45.842] [TRACE] SYS:publish_match ctx=0903710F dest=SYS cmd=LED module=SYS
[14:46:45.847] [TRACE] SYS:route_entry ctx=0903710F cmd=LED args=OFF
[14:46:45.852] [TRACE] SYS:route_ok ctx=0903710F cmd=LED result=+OK:0903710F,SYS:led,off
[14:46:49.775] [TRACE] LORA:QueueRX ctx=030006DE count=0 From=9900 To=FFFF Len=14
[14:46:51.670] [00000] AT Command: AT+PUB=SYS,LED,0x09037110,TOGGLE
[14:46:51.681] [TRACE] SYS:publish_entry ctx=09037110 dest=SYS cmd=LED args=TOGGLE sync=True fanout=False
[14:46:51.685] [TRACE] SYS:publish_match ctx=09037110 dest=SYS cmd=LED module=SYS
[14:46:51.689] [TRACE] SYS:route_entry ctx=09037110 cmd=LED args=TOGGLE
[14:46:51.694] [TRACE] SYS:route_ok ctx=09037110 cmd=LED result=+OK:09037110,SYS:led,toggle
[14:46:58.429] [00000] AT Command: AT+PUB=SYS,LED,0x09037111,OFF
[14:46:58.619] [TRACE] SYS:publish_entry ctx=09037111 dest=SYS cmd=LED args=OFF sync=True fanout=False
[14:46:58.624] [TRACE] SYS:publish_match ctx=09037111 dest=SYS cmd=LED module=SYS
[14:46:58.628] [TRACE] SYS:route_entry ctx=09037111 cmd=LED args=OFF
[14:46:58.635] [TRACE] SYS:route_ok ctx=09037111 cmd=LED result=+OK:09037111,SYS:led,off
[14:46:59.780] [TRACE] LORA:QueueRX ctx=030006E0 count=0 From=9900 To=FFFF Len=14
[14:47:02.742] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:47:04.891] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:47:05.638] [00000] AT Command: AT+PUB=SYS,LED,0x09037112,INFO
[14:47:05.648] [TRACE] SYS:publish_entry ctx=09037112 dest=SYS cmd=LED args=INFO sync=True fanout=False
[14:47:05.653] [TRACE] SYS:publish_match ctx=09037112 dest=SYS cmd=LED module=SYS
[14:47:05.657] [TRACE] SYS:route_entry ctx=09037112 cmd=LED args=INFO
[14:47:05.663] [TRACE] SYS:route_ok ctx=09037112 cmd=LED result=+DATA:09037112,SYS:led,opened=True,pin=18,active=LOW
[14:47:07.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:47:09.511] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12

```
