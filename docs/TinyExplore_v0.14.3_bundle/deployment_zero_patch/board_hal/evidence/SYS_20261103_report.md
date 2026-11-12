# SYS runtime test — 2026-11-03

AEP exchanges completed: 58. ACK confirmed: 58.

Scope: live device behavior; reference contract Foundation v1.0.0. Deployed image identity/hash was not established. This report does not change package verification status.

NEO data=-1 and BUTTON pin=-1: resources absent on this board, confirmed by owner. INFO and idempotent CLOSE tested; physical operation excluded. LED BLINK response tests software acceptance; physical illumination not independently observed.

FLASH uses an exclusively named temporary file in data scope; original scopes and existing files are preserved. Metadata write is an intentional read-only negative test. TMR uses dedicated sys1103 IDs and vacant EMPTY0 route. Race, timing precision, missed-slot and capacity stress tests are outside scope.


Result: PASS within the executed scope. No unexpected command errors observed.

| Area | Observed result |
|---|---|
| SYS INFO / ECHO / LOOPBACK | PASS; context and echo marker retained |
| NEO / BUTTON INFO + CLOSE | PASS; absent hardware, idempotent close already |
| LED BLINK | PASS software response; physical observation excluded |
| FLASH data | PASS; initially absent probe, 17-byte write, exact read, delete, absent again |
| FLASH metadata protection | PASS; EREADONLY |
| One-shot TMR | PASS; START, retained expired state, CLOSE |
| Periodic TMR | PASS; delivered mode=PER counts 1..5, stable run context; next START reset to 1 with new context |
| Period validation | PASS; zero period rejected ERANGE |
| WDG confirmation | PASS; mismatch ignored, matching reply accepted, duplicate not_pending |
| Cleanup | PASS; timer pool empty, test route removed, WDG route count=0, probe deleted |
| RKB / AEP final | PASS; size=80, filter=null, trigger=null, post=0; IDLE |

All 58 exchanges have confirmed ACKs. Captured artifact SHA-256 values rechecked against retained raw bytes. Original observation settings restored; historical buffer contents naturally changed during testing.

## Command evidence

```text
/log?info
+OK
[14:30:19.096] [00000] Command received: /log?info, Method: GET
+OK: loginfo[size=80, head=71, tail=69, index=70, count=79, filter=(null), trigger=(null), post=0]
```

```text
/log?-100
+OK
[14:30:25.367] [00000] Command received: /log?-100, Method: GET
[14:24:39.691] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:24:49.801] [TRACE] LORA:QueueRX ctx=030005D6 count=0 From=9900 To=FFFF Len=14
[14:25:02.722] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0501 To=FFFF Len=13
[14:25:04.861] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:25:07.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:25:10.001] [TRACE] LORA:QueueRX ctx=030005DA count=0 From=9900 To=FFFF Len=14
[14:25:19.802] [TRACE] LORA:QueueRX ctx=030005DC count=0 From=9900 To=FFFF Len=14
[14:25:23.911] [TRACE] LORA:QueueRX ctx=550000B2 count=0 From=5557 To=FFFF Len=11
[14:25:28.081] [TRACE] LORA:QueueRX ctx=5000037C count=0 From=5000 To=FFFF Len=10
[14:25:29.801] [TRACE] LORA:QueueRX ctx=030005DE count=0 From=9900 To=FFFF Len=14
[14:25:32.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:25:34.872] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:25:37.742] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:25:39.993] [TRACE] LORA:QueueRX ctx=030005E0 count=0 From=9900 To=FFFF Len=14
[14:25:50.160] [TRACE] LORA:QueueRX ctx=030005E2 count=0 From=9900 To=FFFF Len=14
[14:25:59.801] [TRACE] LORA:QueueRX ctx=030005E4 count=0 From=9900 To=FFFF Len=14
[14:26:02.721] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:26:04.871] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:26:07.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:26:10.001] [TRACE] LORA:QueueRX ctx=030005E6 count=0 From=9900 To=FFFF Len=14
[14:26:19.803] [TRACE] LORA:QueueRX ctx=030005E8 count=0 From=9900 To=FFFF Len=14
[14:26:23.911] [TRACE] LORA:QueueRX ctx=550000B3 count=0 From=5557 To=FFFF Len=11
[14:26:28.071] [TRACE] LORA:QueueRX ctx=5000037D count=0 From=5000 To=FFFF Len=10
[14:26:32.761] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:26:34.871] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:26:37.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:26:39.991] [TRACE] LORA:QueueRX ctx=030005EC count=0 From=9900 To=FFFF Len=14
[14:26:49.791] [TRACE] LORA:QueueRX ctx=030005EE count=0 From=9900 To=FFFF Len=14
[14:26:59.801] [TRACE] LORA:QueueRX ctx=030005F0 count=0 From=9900 To=FFFF Len=14
[14:27:02.722] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:27:04.862] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:27:07.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:27:09.991] [TRACE] LORA:QueueRX ctx=030005F2 count=0 From=9900 To=FFFF Len=14
[14:27:19.803] [TRACE] LORA:QueueRX ctx=030005F4 count=0 From=9900 To=FFFF Len=14
[14:27:23.911] [TRACE] LORA:QueueRX ctx=550000B4 count=0 From=5557 To=FFFF Len=11
[14:27:28.071] [TRACE] LORA:QueueRX ctx=5000037E count=0 From=5000 To=FFFF Len=10
[14:27:29.793] [TRACE] LORA:QueueRX ctx=030005F6 count=0 From=9900 To=FFFF Len=14
[14:27:32.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:27:34.861] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:27:37.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:27:39.991] [TRACE] LORA:QueueRX ctx=030005F8 count=0 From=9900 To=FFFF Len=14
[14:27:49.791] [TRACE] LORA:QueueRX ctx=030005FA count=0 From=9900 To=FFFF Len=14
[14:27:59.283] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0501 To=FFFF Len=13
[14:28:00.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:28:02.571] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:28:04.871] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:28:07.733] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:28:09.563] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:28:12.771] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0501 To=FFFF Len=13
[14:28:19.803] [TRACE] LORA:QueueRX ctx=03000600 count=0 From=9900 To=FFFF Len=14
[14:28:23.913] [TRACE] LORA:QueueRX ctx=550000B5 count=0 From=5557 To=FFFF Len=11
[14:28:28.071] [TRACE] LORA:QueueRX ctx=5000037F count=0 From=5000 To=FFFF Len=10
[14:28:29.984] [TRACE] LORA:QueueRX ctx=03000602 count=0 From=9900 To=FFFF Len=14
[14:28:32.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:28:34.871] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:28:37.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:28:40.001] [TRACE] LORA:QueueRX ctx=03000604 count=0 From=9900 To=FFFF Len=14
[14:28:49.791] [TRACE] LORA:QueueRX ctx=03000606 count=0 From=9900 To=FFFF Len=14
[14:28:59.801] [TRACE] LORA:QueueRX ctx=03000608 count=0 From=9900 To=FFFF Len=14
[14:29:02.721] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:29:04.873] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:29:07.742] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:29:09.993] [TRACE] LORA:QueueRX ctx=0300060A count=0 From=9900 To=FFFF Len=14
[14:29:19.803] [TRACE] LORA:QueueRX ctx=0300060C count=0 From=9900 To=FFFF Len=14
[14:29:23.913] [TRACE] LORA:QueueRX ctx=550000B6 count=0 From=5557 To=FFFF Len=11
[14:29:28.072] [TRACE] LORA:QueueRX ctx=50000380 count=0 From=5000 To=FFFF Len=10
[14:29:29.794] [TRACE] LORA:QueueRX ctx=0300060E count=0 From=9900 To=FFFF Len=14
[14:29:32.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:29:34.861] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:29:37.741] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:29:39.991] [TRACE] LORA:QueueRX ctx=03000610 count=0 From=9900 To=FFFF Len=14
[14:29:49.791] [TRACE] LORA:QueueRX ctx=03000612 count=0 From=9900 To=FFFF Len=14
[14:30:00.691] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:30:02.521] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:30:04.882] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:30:07.742] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:30:09.724] [TRACE] LORA:QueueRX ctx=03000616 count=0 From=9900 To=FFFF Len=14
[14:30:19.801] [TRACE] LORA:QueueRX ctx=03000618 count=0 From=9900 To=FFFF Len=14
[14:30:23.913] [TRACE] LORA:QueueRX ctx=550000B7 count=0 From=5557 To=FFFF Len=11

```

```text
AT+PUB=SYS,INFO,0x09037001
[14:30:39.562] [00000] AT Command: AT+PUB=SYS,INFO,0x09037001
+DATA:09037001,SYS:name=Sys_SYS,atGroup=SYS,enabled=True,rxCount=10,neo=closed,led=open,button=closed,flash=open
```

```text
AT+PUB=SYS,ECHO,0x09037002,SYS_TEST_20261103
[14:30:47.253] [00000] AT Command: AT+PUB=SYS,ECHO,0x09037002,SYS_TEST_20261103
+DATA:09037002,SYS:SYS_TEST_20261103
```

```text
AT+PUB=SYS,LOOPBACK,0x09037003
[14:30:54.622] [00000] AT Command: AT+PUB=SYS,LOOPBACK,0x09037003
+DATA:09037003,SYS:OK
```

```text
AT+PUB=SYS,NEO,0x09037004,INFO
[14:31:00.363] [00000] AT Command: AT+PUB=SYS,NEO,0x09037004,INFO
+DATA:09037004,SYS:neo,opened=False,data=-1,count=1,power=-1,order=RGB,powerActive=HIGH
```

```text
AT+PUB=SYS,LED,0x09037005,INFO
[14:31:07.772] [00000] AT Command: AT+PUB=SYS,LED,0x09037005,INFO
+DATA:09037005,SYS:led,opened=True,pin=18,active=LOW
```

```text
AT+PUB=SYS,BUTTON,0x09037006,INFO
[14:31:14.582] [00000] AT Command: AT+PUB=SYS,BUTTON,0x09037006,INFO
+DATA:09037006,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
```

```text
AT+PUB=SYS,FLASH,0x09037007,INFO
[14:31:22.363] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037007,INFO
+DATA:09037007,SYS:flash,opened=True,scopes=[metadata|config:RW|data:RW]
```

```text
AT+PUB=SYS,TMR,0x09037008,INFO
[14:31:30.052] [00000] AT Command: AT+PUB=SYS,TMR,0x09037008,INFO
+DATA:09037008,SYS:tmr,activeCount=0,poolCount=0,capacity=10
```

```text
AT+PUB=SYS,TMR,0x09037009,LIST,ALL
[14:31:37.322] [00000] AT Command: AT+PUB=SYS,TMR,0x09037009,LIST,ALL
+DATA:09037009,SYS:tmr,activeCount=0,poolCount=0,capacity=10,[]
```

```text
AT+PUB=SYS,SUBLIST,0x0903700A,EMPTY0,/TMR/EXPIRED,PUSH
[14:31:45.002] [00000] AT Command: AT+PUB=SYS,SUBLIST,0x0903700A,EMPTY0,/TMR/EXPIRED,PUSH
+OK:0903700A,SYS:sub_list,EMPTY0,/TMR/EXPIRED,PUSH,count=0 []
```

```text
AT+PUB=SYS,SUBLIST,0x0903700B,EMPTY0,/WDG/EXPIRED,PUSH
[14:31:52.584] [00000] AT Command: AT+PUB=SYS,SUBLIST,0x0903700B,EMPTY0,/WDG/EXPIRED,PUSH
+OK:0903700B,SYS:sub_list,EMPTY0,/WDG/EXPIRED,PUSH,count=0 []
```

```text
/log?filter=*ctx=09037*
+OK
[14:31:59.326] [00000] Command received: /log?filter=*ctx=09037*, Method: GET
+OK: filter=*ctx=09037*
```

```text
AT+PUB=SYS,FLASH,0x0903700C,INFO,data
[14:32:09.173] [00000] AT Command: AT+PUB=SYS,FLASH,0x0903700C,INFO,data
+DATA:0903700C,SYS:flash,scope=data,mode=RW,recursive=True,quotaBytes=65536,usedBytes=0,availableBytes=65536
```

```text
AT+PUB=SYS,FLASH,0x0903700D,INFO,metadata
[14:32:17.362] [00000] AT Command: AT+PUB=SYS,FLASH,0x0903700D,INFO,metadata
+DATA:0903700D,SYS:flash,scope=metadata,mode=RD,recursive=True
```

```text
AT+PUB=SYS,FLASH,0x0903700E,INFO,config
[14:32:25.142] [00000] AT Command: AT+PUB=SYS,FLASH,0x0903700E,INFO,config
+DATA:0903700E,SYS:flash,scope=config,mode=RW,recursive=False,quotaBytes=65536,usedBytes=0,availableBytes=65536
```

```text
/log?-80
+OK
[14:32:31.088] [00000] Command received: /log?-80, Method: GET
[14:30:47.294] [TRACE] SYS:route_ok ctx=09037002 cmd=ECHO result=+DATA:09037002,SYS:SYS_TEST_20261103
[14:30:49.801] [TRACE] LORA:QueueRX ctx=0300061E count=0 From=9900 To=FFFF Len=14
[14:30:54.630] [00000] AT Command: AT+PUB=SYS,LOOPBACK,0x09037003
[14:30:54.640] [TRACE] SYS:publish_entry ctx=09037003 dest=SYS cmd=LOOPBACK args= sync=True fanout=False
[14:30:54.643] [TRACE] SYS:publish_match ctx=09037003 dest=SYS cmd=LOOPBACK module=SYS
[14:30:54.647] [TRACE] SYS:route_entry ctx=09037003 cmd=LOOPBACK args=
[14:30:54.654] [TRACE] SYS:publish_entry ctx=09037003 dest=SYS cmd=ECHO args=LOOPBACK_TEST sync=True fanout=False
[14:30:54.658] [TRACE] SYS:publish_match ctx=09037003 dest=SYS cmd=ECHO module=SYS
[14:30:54.663] [TRACE] SYS:route_entry ctx=09037003 cmd=ECHO args=LOOPBACK_TEST
[14:30:54.674] [TRACE] SYS:route_ok ctx=09037003 cmd=ECHO result=+DATA:09037003,SYS:LOOPBACK_TEST
[14:30:54.678] [TRACE] SYS:route_ok ctx=09037003 cmd=LOOPBACK result=+DATA:09037003,SYS:OK
[14:30:59.791] [TRACE] LORA:QueueRX ctx=03000620 count=0 From=9900 To=FFFF Len=14
[14:31:00.370] [00000] AT Command: AT+PUB=SYS,NEO,0x09037004,INFO
[14:31:00.381] [TRACE] SYS:publish_entry ctx=09037004 dest=SYS cmd=NEO args=INFO sync=True fanout=False
[14:31:00.385] [TRACE] SYS:publish_match ctx=09037004 dest=SYS cmd=NEO module=SYS
[14:31:00.390] [TRACE] SYS:route_entry ctx=09037004 cmd=NEO args=INFO
[14:31:00.396] [TRACE] SYS:route_ok ctx=09037004 cmd=NEO result=+DATA:09037004,SYS:neo,opened=False,data=-1,count=1,power=-1,order=RGB,powerActive=HIGH
[14:31:02.731] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0903 To=FFFF Len=12
[14:31:04.873] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:31:07.779] [00000] AT Command: AT+PUB=SYS,LED,0x09037005,INFO
[14:31:07.789] [TRACE] SYS:publish_entry ctx=09037005 dest=SYS cmd=LED args=INFO sync=True fanout=False
[14:31:07.793] [TRACE] SYS:publish_match ctx=09037005 dest=SYS cmd=LED module=SYS
[14:31:07.797] [TRACE] SYS:route_entry ctx=09037005 cmd=LED args=INFO
[14:31:07.803] [TRACE] SYS:route_ok ctx=09037005 cmd=LED result=+DATA:09037005,SYS:led,opened=True,pin=18,active=LOW
[14:31:07.831] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:31:09.803] [TRACE] LORA:QueueRX ctx=03000622 count=0 From=9900 To=FFFF Len=14
[14:31:14.588] [00000] AT Command: AT+PUB=SYS,BUTTON,0x09037006,INFO
[14:31:14.599] [TRACE] SYS:publish_entry ctx=09037006 dest=SYS cmd=BUTTON args=INFO sync=True fanout=False
[14:31:14.603] [TRACE] SYS:publish_match ctx=09037006 dest=SYS cmd=BUTTON module=SYS
[14:31:14.607] [TRACE] SYS:route_entry ctx=09037006 cmd=BUTTON args=INFO
[14:31:14.613] [TRACE] SYS:route_ok ctx=09037006 cmd=BUTTON result=+DATA:09037006,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
[14:31:19.783] [TRACE] LORA:QueueRX ctx=03000624 count=0 From=9900 To=FFFF Len=14
[14:31:22.369] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037007,INFO
[14:31:22.380] [TRACE] SYS:publish_entry ctx=09037007 dest=SYS cmd=FLASH args=INFO sync=True fanout=False
[14:31:22.383] [TRACE] SYS:publish_match ctx=09037007 dest=SYS cmd=FLASH module=SYS
[14:31:22.388] [TRACE] SYS:route_entry ctx=09037007 cmd=FLASH args=INFO
[14:31:22.393] [TRACE] SYS:route_ok ctx=09037007 cmd=FLASH result=+DATA:09037007,SYS:flash,opened=True,scopes=[metadata|config:RW|data:RW]
[14:31:23.912] [TRACE] LORA:QueueRX ctx=550000B8 count=0 From=5557 To=FFFF Len=11
[14:31:28.071] [TRACE] LORA:QueueRX ctx=50000382 count=0 From=5000 To=FFFF Len=10
[14:31:30.060] [00000] AT Command: AT+PUB=SYS,TMR,0x09037008,INFO
[14:31:30.071] [TRACE] SYS:publish_entry ctx=09037008 dest=SYS cmd=TMR args=INFO sync=True fanout=False
[14:31:30.075] [TRACE] SYS:publish_match ctx=09037008 dest=SYS cmd=TMR module=SYS
[14:31:30.081] [TRACE] SYS:route_entry ctx=09037008 cmd=TMR args=INFO
[14:31:30.088] [TRACE] SYS:route_ok ctx=09037008 cmd=TMR result=+DATA:09037008,SYS:tmr,activeCount=0,poolCount=0,capacity=10
[14:31:32.861] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=3901 To=FFFF Len=12
[14:31:34.891] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0800 To=FFFF Len=11
[14:31:37.328] [00000] AT Command: AT+PUB=SYS,TMR,0x09037009,LIST,ALL
[14:31:37.339] [TRACE] SYS:publish_entry ctx=09037009 dest=SYS cmd=TMR args=LIST,ALL sync=True fanout=False
[14:31:37.343] [TRACE] SYS:publish_match ctx=09037009 dest=SYS cmd=TMR module=SYS
[14:31:37.348] [TRACE] SYS:route_entry ctx=09037009 cmd=TMR args=LIST,ALL
[14:31:37.356] [TRACE] SYS:route_ok ctx=09037009 cmd=TMR result=+DATA:09037009,SYS:tmr,activeCount=0,poolCount=0,capacity=10,[]
[14:31:38.141] [TRACE] LORA:QueueRX ctx=00000000 count=0 From=0900 To=FFFF Len=11
[14:31:40.011] [TRACE] LORA:QueueRX ctx=03000628 count=0 From=9900 To=FFFF Len=14
[14:31:45.010] [00000] AT Command: AT+PUB=SYS,SUBLIST,0x0903700A,EMPTY0,/TMR/EXPIRED,PUSH
[14:31:45.023] [TRACE] SYS:publish_entry ctx=0903700A dest=SYS cmd=SUBLIST args=EMPTY0,/TMR/EXPIRED,PUSH sync=True fanout=False
[14:31:45.027] [TRACE] SYS:publish_match ctx=0903700A dest=SYS cmd=SUBLIST module=SYS
[14:31:45.032] [TRACE] SYS:route_entry ctx=0903700A cmd=SUBLIST args=EMPTY0,/TMR/EXPIRED,PUSH
[14:31:45.041] [TRACE] SYS:crosssub_list ctx=0903700A consumer=EMPTY0 topic=/TMR/EXPIRED mode=PUSH count=0 []
[14:31:45.046] [TRACE] SYS:route_default ctx=0903700A cmd=SUBLIST result=+OK:0903700A,SYS:sub_list,EMPTY0,/TMR/EXPIRED,PUSH,count=0 []
[14:31:49.791] [TRACE] LORA:QueueRX ctx=0300062A count=0 From=9900 To=FFFF Len=14
[14:31:52.591] [00000] AT Command: AT+PUB=SYS,SUBLIST,0x0903700B,EMPTY0,/WDG/EXPIRED,PUSH
[14:31:52.604] [TRACE] SYS:publish_entry ctx=0903700B dest=SYS cmd=SUBLIST args=EMPTY0,/WDG/EXPIRED,PUSH sync=True fanout=False
[14:31:52.607] [TRACE] SYS:publish_match ctx=0903700B dest=SYS cmd=SUBLIST module=SYS
[14:31:52.612] [TRACE] SYS:route_entry ctx=0903700B cmd=SUBLIST args=EMPTY0,/WDG/EXPIRED,PUSH
[14:31:52.622] [TRACE] SYS:crosssub_list ctx=0903700B consumer=EMPTY0 topic=/WDG/EXPIRED mode=PUSH count=0 []
[14:31:52.626] [TRACE] SYS:route_default ctx=0903700B cmd=SUBLIST result=+OK:0903700B,SYS:sub_list,EMPTY0,/WDG/EXPIRED,PUSH,count=0 []
~[14:31:59.336] [AppLog] FILTER: filter set to '*ctx=09037*'
~[14:32:09.197] [TRACE] SYS:publish_entry ctx=0903700C dest=SYS cmd=FLASH args=INFO,data sync=True fanout=False
~[14:32:09.205] [TRACE] SYS:publish_match ctx=0903700C dest=SYS cmd=FLASH module=SYS
~[14:32:09.215] [TRACE] SYS:route_entry ctx=0903700C cmd=FLASH args=INFO,data
~[14:32:09.267] [TRACE] SYS:route_ok ctx=0903700C cmd=FLASH result=+DATA:0903700C,SYS:flash,scope=data,mode=RW,recursive=True,quotaBytes=65536,usedBytes=0,availableBytes=65536
~[14:32:17.387] [TRACE] SYS:publish_entry ctx=0903700D dest=SYS cmd=FLASH args=INFO,metadata sync=True fanout=False
~[14:32:17.396] [TRACE] SYS:publish_match ctx=0903700D dest=SYS cmd=FLASH module=SYS
~[14:32:17.405] [TRACE] SYS:route_entry ctx=0903700D cmd=FLASH args=INFO,metadata
~[14:32:17.415] [TRACE] SYS:route_ok ctx=0903700D cmd=FLASH result=+DATA:0903700D,SYS:flash,scope=metadata,mode=RD,recursive=True
~[14:32:25.167] [TRACE] SYS:publish_entry ctx=0903700E dest=SYS cmd=FLASH args=INFO,config sync=True fanout=False
~[14:32:25.176] [TRACE] SYS:publish_match ctx=0903700E dest=SYS cmd=FLASH module=SYS
~[14:32:25.185] [TRACE] SYS:route_entry ctx=0903700E cmd=FLASH args=INFO,config
~[14:32:25.221] [TRACE] SYS:route_ok ctx=0903700E cmd=FLASH result=+DATA:0903700E,SYS:flash,scope=config,mode=RW,recursive=False,quotaBytes=65536,usedBytes=0,availableBytes=65536

```

```text
AT+PUB=SYS,LED,0x09037010,BLINK,2,25
[14:34:11.142] [00000] AT Command: AT+PUB=SYS,LED,0x09037010,BLINK,2,25
+OK:09037010,SYS:led,blink,count=2,phaseMs=25
```

```text
AT+PUB=SYS,NEO,0x09037011,OFF
[14:34:19.422] [00000] AT Command: AT+PUB=SYS,NEO,0x09037011,OFF
+ERR:09037011,SYS,ENOTOPEN
```

```text
AT+PUB=SYS,FLASH,0x09037012,EXISTS,data,sys1103_probe.txt
[14:34:26.282] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037012,EXISTS,data,sys1103_probe.txt
+DATA:09037012,SYS:flash,exists,data,sys1103_probe.txt,false
```

```text
AT+PUB=SYS,FLASH,0x09037013,WRITE,data,sys1103_probe.txt,SYS_TEST_20261103
[14:34:34.174] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037013,WRITE,data,sys1103_probe.txt,SYS_TEST_20261103
+OK:09037013,SYS:flash,write,data,sys1103_probe.txt,bytes=17,available=65519
```

```text
AT+PUB=SYS,FLASH,0x09037014,READ,data,sys1103_probe.txt
[14:34:42.262] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037014,READ,data,sys1103_probe.txt
+DATA:09037014,SYS:flash,read,data,sys1103_probe.txt,SYS_TEST_20261103
```

```text
AT+PUB=SYS,FLASH,0x09037015,DELETE,data,sys1103_probe.txt
[14:34:50.641] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037015,DELETE,data,sys1103_probe.txt
+OK:09037015,SYS:flash,delete,data,sys1103_probe.txt,bytes=17,available=65536
```

```text
AT+PUB=SYS,NEO,0x09037020,INFO
[14:35:27.034] [00000] AT Command: AT+PUB=SYS,NEO,0x09037020,INFO
+DATA:09037020,SYS:neo,opened=False,data=-1,count=1,power=-1,order=RGB,powerActive=HIGH
```

```text
AT+PUB=SYS,BUTTON,0x09037021,INFO
[14:35:33.772] [00000] AT Command: AT+PUB=SYS,BUTTON,0x09037021,INFO
+DATA:09037021,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
```

```text
AT+PUB=SYS,NEO,0x09037022,CLOSE
[14:35:39.868] [00000] AT Command: AT+PUB=SYS,NEO,0x09037022,CLOSE
+OK:09037022,SYS:neo,close,already
```

```text
AT+PUB=SYS,BUTTON,0x09037023,CLOSE
[14:35:47.692] [00000] AT Command: AT+PUB=SYS,BUTTON,0x09037023,CLOSE
+OK:09037023,SYS:button,close,already
```

```text
AT+PUB=SYS,NEO,0x09037024,INFO
[14:35:55.362] [00000] AT Command: AT+PUB=SYS,NEO,0x09037024,INFO
+DATA:09037024,SYS:neo,opened=False,data=-1,count=1,power=-1,order=RGB,powerActive=HIGH
```

```text
AT+PUB=SYS,BUTTON,0x09037025,INFO
[14:36:02.844] [00000] AT Command: AT+PUB=SYS,BUTTON,0x09037025,INFO
+DATA:09037025,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
```

```text
AT+PUB=SYS,FLASH,0x09037016,EXISTS,data,sys1103_probe.txt
[14:36:09.521] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037016,EXISTS,data,sys1103_probe.txt
+DATA:09037016,SYS:flash,exists,data,sys1103_probe.txt,false
```

```text
AT+PUB=SYS,FLASH,0x09037017,WRITE,metadata,sys1103_probe.txt,probe
[14:36:16.784] [00000] AT Command: AT+PUB=SYS,FLASH,0x09037017,WRITE,metadata,sys1103_probe.txt,probe
+ERR:09037017,SYS,EREADONLY
```

```text
AT+PUB=SYS,TMR,0x09037018,OPEN,sys1103_one,100,probe
[14:36:24.353] [00000] AT Command: AT+PUB=SYS,TMR,0x09037018,OPEN,sys1103_one,100,probe
+OK:09037018,SYS:tmr,open,sys1103_one
```

```text
AT+PUB=SYS,TMR,0x09037019,START,sys1103_one
[14:36:30.703] [00000] AT Command: AT+PUB=SYS,TMR,0x09037019,START,sys1103_one
+OK:09037019,SYS:tmr,start,sys1103_one
```

```text
AT+PUB=SYS,TMR,0x0903701A,LIST,ALL
[14:36:37.959] [00000] AT Command: AT+PUB=SYS,TMR,0x0903701A,LIST,ALL
+DATA:0903701A,SYS:tmr,activeCount=0,poolCount=1,capacity=10,[sys1103_one,expired,100]
```

```text
AT+PUB=SYS,TMR,0x0903701B,CLOSE,sys1103_one
[14:36:45.852] [00000] AT Command: AT+PUB=SYS,TMR,0x0903701B,CLOSE,sys1103_one
+OK:0903701B,SYS:tmr,close,sys1103_one
```

```text
AT+PUB=SYS,SUB,0x09037030,EMPTY0,/TMR/EXPIRED,$arg0=sys1103_per,PUSH
[14:36:52.644] [00000] AT Command: AT+PUB=SYS,SUB,0x09037030,EMPTY0,/TMR/EXPIRED,$arg0=sys1103_per,PUSH
+OK:09037030,SYS:sub,/TMR/EXPIRED|$arg0=sys1103_per
```

```text
AT+PUB=SYS,TMR,0x09037031,OPEN,sys1103_per,100,probe,3000
[14:37:00.260] [00000] AT Command: AT+PUB=SYS,TMR,0x09037031,OPEN,sys1103_per,100,probe,3000
+OK:09037031,SYS:tmr,open,sys1103_per
```

```text
AT+PUB=SYS,TMR,0x09037032,START,sys1103_per
[14:37:07.914] [00000] AT Command: AT+PUB=SYS,TMR,0x09037032,START,sys1103_per
+OK:09037032,SYS:tmr,start,sys1103_per
```

```text
AT+PUB=SYS,TMR,0x09037033,INFO,sys1103_per
[14:37:15.164] [00000] AT Command: AT+PUB=SYS,TMR,0x09037033,INFO,sys1103_per
+DATA:09037033,SYS:tmr,id=sys1103_per,state=active,delayMs=100,type=TMR,consumer=,remainingMs=1850,tag=probe,context=09037032,mode=PERIODIC,periodMs=3000,delivering=false
```

```text
AT+PUB=SYS,TMR,0x09037034,CANCEL,sys1103_per
[14:37:21.094] [00000] AT Command: AT+PUB=SYS,TMR,0x09037034,CANCEL,sys1103_per
+OK:09037034,SYS:tmr,cancel,sys1103_per
```

```text
/log?-80
+OK
[14:37:30.118] [00000] Command received: /log?-80, Method: GET
~[14:35:33.804] [TRACE] SYS:publish_match ctx=09037021 dest=SYS cmd=BUTTON module=SYS
~[14:35:33.813] [TRACE] SYS:route_entry ctx=09037021 cmd=BUTTON args=INFO
~[14:35:33.824] [TRACE] SYS:route_ok ctx=09037021 cmd=BUTTON result=+DATA:09037021,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
~[14:35:39.920] [TRACE] SYS:publish_entry ctx=09037022 dest=SYS cmd=NEO args=CLOSE sync=True fanout=False
~[14:35:39.929] [TRACE] SYS:publish_match ctx=09037022 dest=SYS cmd=NEO module=SYS
~[14:35:39.938] [TRACE] SYS:route_entry ctx=09037022 cmd=NEO args=CLOSE
~[14:35:39.948] [TRACE] SYS:route_ok ctx=09037022 cmd=NEO result=+OK:09037022,SYS:neo,close,already
~[14:35:47.714] [TRACE] SYS:publish_entry ctx=09037023 dest=SYS cmd=BUTTON args=CLOSE sync=True fanout=False
~[14:35:47.722] [TRACE] SYS:publish_match ctx=09037023 dest=SYS cmd=BUTTON module=SYS
~[14:35:47.732] [TRACE] SYS:route_entry ctx=09037023 cmd=BUTTON args=CLOSE
~[14:35:47.741] [TRACE] SYS:route_ok ctx=09037023 cmd=BUTTON result=+OK:09037023,SYS:button,close,already
~[14:35:55.384] [TRACE] SYS:publish_entry ctx=09037024 dest=SYS cmd=NEO args=INFO sync=True fanout=False
~[14:35:55.392] [TRACE] SYS:publish_match ctx=09037024 dest=SYS cmd=NEO module=SYS
~[14:35:55.401] [TRACE] SYS:route_entry ctx=09037024 cmd=NEO args=INFO
~[14:35:55.412] [TRACE] SYS:route_ok ctx=09037024 cmd=NEO result=+DATA:09037024,SYS:neo,opened=False,data=-1,count=1,power=-1,order=RGB,powerActive=HIGH
~[14:36:02.899] [TRACE] SYS:publish_entry ctx=09037025 dest=SYS cmd=BUTTON args=INFO sync=True fanout=False
~[14:36:02.907] [TRACE] SYS:publish_match ctx=09037025 dest=SYS cmd=BUTTON module=SYS
~[14:36:02.930] [TRACE] SYS:route_entry ctx=09037025 cmd=BUTTON args=INFO
~[14:36:02.941] [TRACE] SYS:route_ok ctx=09037025 cmd=BUTTON result=+DATA:09037025,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
~[14:36:09.569] [TRACE] SYS:publish_entry ctx=09037016 dest=SYS cmd=FLASH args=EXISTS,data,sys1103_probe.txt sync=True fanout=False
~[14:36:09.578] [TRACE] SYS:publish_match ctx=09037016 dest=SYS cmd=FLASH module=SYS
~[14:36:09.588] [TRACE] SYS:route_entry ctx=09037016 cmd=FLASH args=EXISTS,data,sys1103_probe.txt
~[14:36:09.634] [TRACE] SYS:route_ok ctx=09037016 cmd=FLASH result=+DATA:09037016,SYS:flash,exists,data,sys1103_probe.txt,false
~[14:36:16.813] [TRACE] SYS:publish_entry ctx=09037017 dest=SYS cmd=FLASH args=WRITE,metadata,sys1103_probe.txt,probe sync=True fanout=False
~[14:36:16.822] [TRACE] SYS:publish_match ctx=09037017 dest=SYS cmd=FLASH module=SYS
~[14:36:16.832] [TRACE] SYS:route_entry ctx=09037017 cmd=FLASH args=WRITE,metadata,sys1103_probe.txt,probe
~[14:36:16.844] [TRACE] SYS:route_ok ctx=09037017 cmd=FLASH result=+ERR:09037017,SYS,EREADONLY
~[14:36:24.379] [TRACE] SYS:publish_entry ctx=09037018 dest=SYS cmd=TMR args=OPEN,sys1103_one,100,probe sync=True fanout=False
~[14:36:24.389] [TRACE] SYS:publish_match ctx=09037018 dest=SYS cmd=TMR module=SYS
~[14:36:24.400] [TRACE] SYS:route_entry ctx=09037018 cmd=TMR args=OPEN,sys1103_one,100,probe
~[14:36:24.413] [TRACE] SYS:route_ok ctx=09037018 cmd=TMR result=+OK:09037018,SYS:tmr,open,sys1103_one
~[14:36:30.728] [TRACE] SYS:publish_entry ctx=09037019 dest=SYS cmd=TMR args=START,sys1103_one sync=True fanout=False
~[14:36:30.736] [TRACE] SYS:publish_match ctx=09037019 dest=SYS cmd=TMR module=SYS
~[14:36:30.746] [TRACE] SYS:route_entry ctx=09037019 cmd=TMR args=START,sys1103_one
~[14:36:30.758] [TRACE] SYS:route_ok ctx=09037019 cmd=TMR result=+OK:09037019,SYS:tmr,start,sys1103_one
~[14:36:37.998] [TRACE] SYS:publish_entry ctx=0903701A dest=SYS cmd=TMR args=LIST,ALL sync=True fanout=False
~[14:36:38.007] [TRACE] SYS:publish_match ctx=0903701A dest=SYS cmd=TMR module=SYS
~[14:36:38.017] [TRACE] SYS:route_entry ctx=0903701A cmd=TMR args=LIST,ALL
~[14:36:38.054] [TRACE] SYS:route_ok ctx=0903701A cmd=TMR result=+DATA:0903701A,SYS:tmr,activeCount=0,poolCount=1,capacity=10,[sys1103_one,expired,100]
~[14:36:45.878] [TRACE] SYS:publish_entry ctx=0903701B dest=SYS cmd=TMR args=CLOSE,sys1103_one sync=True fanout=False
~[14:36:45.886] [TRACE] SYS:publish_match ctx=0903701B dest=SYS cmd=TMR module=SYS
~[14:36:45.896] [TRACE] SYS:route_entry ctx=0903701B cmd=TMR args=CLOSE,sys1103_one
~[14:36:45.907] [TRACE] SYS:route_ok ctx=0903701B cmd=TMR result=+OK:0903701B,SYS:tmr,close,sys1103_one
~[14:36:52.673] [TRACE] SYS:publish_entry ctx=09037030 dest=SYS cmd=SUB args=EMPTY0,/TMR/EXPIRED,$arg0=sys1103_per,PUSH sync=True fanout=False
~[14:36:52.682] [TRACE] SYS:publish_match ctx=09037030 dest=SYS cmd=SUB module=SYS
~[14:36:52.693] [TRACE] SYS:route_entry ctx=09037030 cmd=SUB args=EMPTY0,/TMR/EXPIRED,$arg0=sys1103_per,PUSH
~[14:36:52.712] [TRACE] SYS:crosssub_add ctx=09037030 consumer=EMPTY0 topic=/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:36:52.722] [TRACE] SYS:route_default ctx=09037030 cmd=SUB result=+OK:09037030,SYS:sub,/TMR/EXPIRED|$arg0=sys1103_per
~[14:37:00.286] [TRACE] SYS:publish_entry ctx=09037031 dest=SYS cmd=TMR args=OPEN,sys1103_per,100,probe,3000 sync=True fanout=False
~[14:37:00.295] [TRACE] SYS:publish_match ctx=09037031 dest=SYS cmd=TMR module=SYS
~[14:37:00.305] [TRACE] SYS:route_entry ctx=09037031 cmd=TMR args=OPEN,sys1103_per,100,probe,3000
~[14:37:00.319] [TRACE] SYS:route_ok ctx=09037031 cmd=TMR result=+OK:09037031,SYS:tmr,open,sys1103_per
~[14:37:07.941] [TRACE] SYS:publish_entry ctx=09037032 dest=SYS cmd=TMR args=START,sys1103_per sync=True fanout=False
~[14:37:07.950] [TRACE] SYS:publish_match ctx=09037032 dest=SYS cmd=TMR module=SYS
~[14:37:07.960] [TRACE] SYS:route_entry ctx=09037032 cmd=TMR args=START,sys1103_per
~[14:37:07.972] [TRACE] SYS:route_ok ctx=09037032 cmd=TMR result=+OK:09037032,SYS:tmr,start,sys1103_per
~[14:37:08.604] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:08.624] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:08.637] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=1
~[14:37:11.073] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:11.092] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:11.105] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=2
~[14:37:14.075] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:14.094] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:14.107] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=3
~[14:37:15.188] [TRACE] SYS:publish_entry ctx=09037033 dest=SYS cmd=TMR args=INFO,sys1103_per sync=True fanout=False
~[14:37:15.196] [TRACE] SYS:publish_match ctx=09037033 dest=SYS cmd=TMR module=SYS
~[14:37:15.206] [TRACE] SYS:route_entry ctx=09037033 cmd=TMR args=INFO,sys1103_per
~[14:37:15.218] [TRACE] SYS:route_ok ctx=09037033 cmd=TMR result=+DATA:09037033,SYS:tmr,id=sys1103_per,state=active,delayMs=100,type=TMR,consumer=,remainingMs=1850,tag=probe,context=09037032,mode=PERIODIC,periodMs=3000,delivering=false
~[14:37:17.072] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:17.091] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:17.104] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=4
~[14:37:20.073] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:20.092] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:20.105] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=5
~[14:37:21.121] [TRACE] SYS:publish_entry ctx=09037034 dest=SYS cmd=TMR args=CANCEL,sys1103_per sync=True fanout=False
~[14:37:21.129] [TRACE] SYS:publish_match ctx=09037034 dest=SYS cmd=TMR module=SYS
~[14:37:21.139] [TRACE] SYS:route_entry ctx=09037034 cmd=TMR args=CANCEL,sys1103_per
~[14:37:21.151] [TRACE] SYS:route_ok ctx=09037034 cmd=TMR result=+OK:09037034,SYS:tmr,cancel,sys1103_per

```

```text
AT+PUB=SYS,TMR,0x09037035,START,sys1103_per
[14:37:46.212] [00000] AT Command: AT+PUB=SYS,TMR,0x09037035,START,sys1103_per
+OK:09037035,SYS:tmr,start,sys1103_per
```

```text
AT+PUB=SYS,TMR,0x09037036,CANCEL,sys1103_per
[14:37:55.692] [00000] AT Command: AT+PUB=SYS,TMR,0x09037036,CANCEL,sys1103_per
+OK:09037036,SYS:tmr,cancel,sys1103_per
```

```text
/log?-80
+OK
[14:38:02.091] [00000] Command received: /log?-80, Method: GET
~[14:36:02.930] [TRACE] SYS:route_entry ctx=09037025 cmd=BUTTON args=INFO
~[14:36:02.941] [TRACE] SYS:route_ok ctx=09037025 cmd=BUTTON result=+DATA:09037025,SYS:button,opened=False,pin=-1,mode=2,debounceMs=333,holding=False
~[14:36:09.569] [TRACE] SYS:publish_entry ctx=09037016 dest=SYS cmd=FLASH args=EXISTS,data,sys1103_probe.txt sync=True fanout=False
~[14:36:09.578] [TRACE] SYS:publish_match ctx=09037016 dest=SYS cmd=FLASH module=SYS
~[14:36:09.588] [TRACE] SYS:route_entry ctx=09037016 cmd=FLASH args=EXISTS,data,sys1103_probe.txt
~[14:36:09.634] [TRACE] SYS:route_ok ctx=09037016 cmd=FLASH result=+DATA:09037016,SYS:flash,exists,data,sys1103_probe.txt,false
~[14:36:16.813] [TRACE] SYS:publish_entry ctx=09037017 dest=SYS cmd=FLASH args=WRITE,metadata,sys1103_probe.txt,probe sync=True fanout=False
~[14:36:16.822] [TRACE] SYS:publish_match ctx=09037017 dest=SYS cmd=FLASH module=SYS
~[14:36:16.832] [TRACE] SYS:route_entry ctx=09037017 cmd=FLASH args=WRITE,metadata,sys1103_probe.txt,probe
~[14:36:16.844] [TRACE] SYS:route_ok ctx=09037017 cmd=FLASH result=+ERR:09037017,SYS,EREADONLY
~[14:36:24.379] [TRACE] SYS:publish_entry ctx=09037018 dest=SYS cmd=TMR args=OPEN,sys1103_one,100,probe sync=True fanout=False
~[14:36:24.389] [TRACE] SYS:publish_match ctx=09037018 dest=SYS cmd=TMR module=SYS
~[14:36:24.400] [TRACE] SYS:route_entry ctx=09037018 cmd=TMR args=OPEN,sys1103_one,100,probe
~[14:36:24.413] [TRACE] SYS:route_ok ctx=09037018 cmd=TMR result=+OK:09037018,SYS:tmr,open,sys1103_one
~[14:36:30.728] [TRACE] SYS:publish_entry ctx=09037019 dest=SYS cmd=TMR args=START,sys1103_one sync=True fanout=False
~[14:36:30.736] [TRACE] SYS:publish_match ctx=09037019 dest=SYS cmd=TMR module=SYS
~[14:36:30.746] [TRACE] SYS:route_entry ctx=09037019 cmd=TMR args=START,sys1103_one
~[14:36:30.758] [TRACE] SYS:route_ok ctx=09037019 cmd=TMR result=+OK:09037019,SYS:tmr,start,sys1103_one
~[14:36:37.998] [TRACE] SYS:publish_entry ctx=0903701A dest=SYS cmd=TMR args=LIST,ALL sync=True fanout=False
~[14:36:38.007] [TRACE] SYS:publish_match ctx=0903701A dest=SYS cmd=TMR module=SYS
~[14:36:38.017] [TRACE] SYS:route_entry ctx=0903701A cmd=TMR args=LIST,ALL
~[14:36:38.054] [TRACE] SYS:route_ok ctx=0903701A cmd=TMR result=+DATA:0903701A,SYS:tmr,activeCount=0,poolCount=1,capacity=10,[sys1103_one,expired,100]
~[14:36:45.878] [TRACE] SYS:publish_entry ctx=0903701B dest=SYS cmd=TMR args=CLOSE,sys1103_one sync=True fanout=False
~[14:36:45.886] [TRACE] SYS:publish_match ctx=0903701B dest=SYS cmd=TMR module=SYS
~[14:36:45.896] [TRACE] SYS:route_entry ctx=0903701B cmd=TMR args=CLOSE,sys1103_one
~[14:36:45.907] [TRACE] SYS:route_ok ctx=0903701B cmd=TMR result=+OK:0903701B,SYS:tmr,close,sys1103_one
~[14:36:52.673] [TRACE] SYS:publish_entry ctx=09037030 dest=SYS cmd=SUB args=EMPTY0,/TMR/EXPIRED,$arg0=sys1103_per,PUSH sync=True fanout=False
~[14:36:52.682] [TRACE] SYS:publish_match ctx=09037030 dest=SYS cmd=SUB module=SYS
~[14:36:52.693] [TRACE] SYS:route_entry ctx=09037030 cmd=SUB args=EMPTY0,/TMR/EXPIRED,$arg0=sys1103_per,PUSH
~[14:36:52.712] [TRACE] SYS:crosssub_add ctx=09037030 consumer=EMPTY0 topic=/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:36:52.722] [TRACE] SYS:route_default ctx=09037030 cmd=SUB result=+OK:09037030,SYS:sub,/TMR/EXPIRED|$arg0=sys1103_per
~[14:37:00.286] [TRACE] SYS:publish_entry ctx=09037031 dest=SYS cmd=TMR args=OPEN,sys1103_per,100,probe,3000 sync=True fanout=False
~[14:37:00.295] [TRACE] SYS:publish_match ctx=09037031 dest=SYS cmd=TMR module=SYS
~[14:37:00.305] [TRACE] SYS:route_entry ctx=09037031 cmd=TMR args=OPEN,sys1103_per,100,probe,3000
~[14:37:00.319] [TRACE] SYS:route_ok ctx=09037031 cmd=TMR result=+OK:09037031,SYS:tmr,open,sys1103_per
~[14:37:07.941] [TRACE] SYS:publish_entry ctx=09037032 dest=SYS cmd=TMR args=START,sys1103_per sync=True fanout=False
~[14:37:07.950] [TRACE] SYS:publish_match ctx=09037032 dest=SYS cmd=TMR module=SYS
~[14:37:07.960] [TRACE] SYS:route_entry ctx=09037032 cmd=TMR args=START,sys1103_per
~[14:37:07.972] [TRACE] SYS:route_ok ctx=09037032 cmd=TMR result=+OK:09037032,SYS:tmr,start,sys1103_per
~[14:37:08.604] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:08.624] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:08.637] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=1
~[14:37:11.073] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:11.092] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:11.105] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=2
~[14:37:14.075] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:14.094] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:14.107] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=3
~[14:37:15.188] [TRACE] SYS:publish_entry ctx=09037033 dest=SYS cmd=TMR args=INFO,sys1103_per sync=True fanout=False
~[14:37:15.196] [TRACE] SYS:publish_match ctx=09037033 dest=SYS cmd=TMR module=SYS
~[14:37:15.206] [TRACE] SYS:route_entry ctx=09037033 cmd=TMR args=INFO,sys1103_per
~[14:37:15.218] [TRACE] SYS:route_ok ctx=09037033 cmd=TMR result=+DATA:09037033,SYS:tmr,id=sys1103_per,state=active,delayMs=100,type=TMR,consumer=,remainingMs=1850,tag=probe,context=09037032,mode=PERIODIC,periodMs=3000,delivering=false
~[14:37:17.072] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:17.091] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:17.104] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=4
~[14:37:20.073] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:20.092] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:20.105] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=5
~[14:37:21.121] [TRACE] SYS:publish_entry ctx=09037034 dest=SYS cmd=TMR args=CANCEL,sys1103_per sync=True fanout=False
~[14:37:21.129] [TRACE] SYS:publish_match ctx=09037034 dest=SYS cmd=TMR module=SYS
~[14:37:21.139] [TRACE] SYS:route_entry ctx=09037034 cmd=TMR args=CANCEL,sys1103_per
~[14:37:21.151] [TRACE] SYS:route_ok ctx=09037034 cmd=TMR result=+OK:09037034,SYS:tmr,cancel,sys1103_per
~[14:37:46.239] [TRACE] SYS:publish_entry ctx=09037035 dest=SYS cmd=TMR args=START,sys1103_per sync=True fanout=False
~[14:37:46.248] [TRACE] SYS:publish_match ctx=09037035 dest=SYS cmd=TMR module=SYS
~[14:37:46.258] [TRACE] SYS:route_entry ctx=09037035 cmd=TMR args=START,sys1103_per
~[14:37:46.269] [TRACE] SYS:route_ok ctx=09037035 cmd=TMR result=+OK:09037035,SYS:tmr,start,sys1103_per
~[14:37:46.703] [TRACE] SYS:emit_event ctx=09037035 topic=SYS/TMR/EXPIRED
~[14:37:46.722] [TRACE] EMPTY0:deliver_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:46.735] [TRACE] EMPTY0:handler_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=1
~[14:37:49.372] [TRACE] SYS:emit_event ctx=09037035 topic=SYS/TMR/EXPIRED
~[14:37:49.391] [TRACE] EMPTY0:deliver_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:49.404] [TRACE] EMPTY0:handler_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=2
~[14:37:52.374] [TRACE] SYS:emit_event ctx=09037035 topic=SYS/TMR/EXPIRED
~[14:37:52.393] [TRACE] EMPTY0:deliver_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:52.406] [TRACE] EMPTY0:handler_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=3
~[14:37:55.719] [TRACE] SYS:publish_entry ctx=09037036 dest=SYS cmd=TMR args=CANCEL,sys1103_per sync=True fanout=False
~[14:37:55.727] [TRACE] SYS:publish_match ctx=09037036 dest=SYS cmd=TMR module=SYS
~[14:37:55.737] [TRACE] SYS:route_entry ctx=09037036 cmd=TMR args=CANCEL,sys1103_per
~[14:37:55.748] [TRACE] SYS:route_ok ctx=09037036 cmd=TMR result=+OK:09037036,SYS:tmr,cancel,sys1103_per

```

```text
AT+PUB=SYS,TMR,0x09037037,CLOSE,sys1103_per
[14:38:17.602] [00000] AT Command: AT+PUB=SYS,TMR,0x09037037,CLOSE,sys1103_per
+OK:09037037,SYS:tmr,close,sys1103_per
```

```text
AT+PUB=SYS,UNSUB,0x09037038,EMPTY0,/TMR/EXPIRED,PUSH
[14:38:25.182] [00000] AT Command: AT+PUB=SYS,UNSUB,0x09037038,EMPTY0,/TMR/EXPIRED,PUSH
+OK:09037038,SYS:unsub,EMPTY0,/TMR/EXPIRED,PUSH,removed=1
```

```text
AT+PUB=SYS,TMR,0x09037039,OPEN,sys1103_bad,100,probe,0
[14:38:33.174] [00000] AT Command: AT+PUB=SYS,TMR,0x09037039,OPEN,sys1103_bad,100,probe,0
+ERR:09037039,SYS,ERANGE
```

```text
AT+PUB=SYS,TMR,0x09037040,WDG,sys1103_wdg,60000,EMPTY0,probe,ACK,act
[14:38:40.344] [00000] AT Command: AT+PUB=SYS,TMR,0x09037040,WDG,sys1103_wdg,60000,EMPTY0,probe,ACK,act
+OK:09037040,SYS:tmr,wdg,sys1103_wdg
```

```text
AT+PUB=SYS,TMR,0x09037041,WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,BAD
[14:38:47.202] [00000] AT Command: AT+PUB=SYS,TMR,0x09037041,WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,BAD
+DATA:09037041,SYS:tmr,wdg_confirm,sys1103_wdg,ignored,reason=reply_mismatch
```

```text
AT+PUB=SYS,TMR,0x09037042,WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK
[14:38:53.343] [00000] AT Command: AT+PUB=SYS,TMR,0x09037042,WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK
+DATA:09037042,SYS:tmr,wdg_confirm,sys1103_wdg,accepted
```

```text
AT+PUB=SYS,TMR,0x09037043,WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK
[14:39:00.203] [00000] AT Command: AT+PUB=SYS,TMR,0x09037043,WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK
+DATA:09037043,SYS:tmr,wdg_confirm,sys1103_wdg,ignored,reason=not_pending
```

```text
AT+PUB=SYS,TMR,0x09037044,CLOSE,sys1103_wdg
[14:39:07.873] [00000] AT Command: AT+PUB=SYS,TMR,0x09037044,CLOSE,sys1103_wdg
+OK:09037044,SYS:tmr,close,sys1103_wdg
```

```text
AT+PUB=SYS,SUBLIST,0x09037045,EMPTY0,/WDG/EXPIRED,PUSH
[14:39:14.232] [00000] AT Command: AT+PUB=SYS,SUBLIST,0x09037045,EMPTY0,/WDG/EXPIRED,PUSH
+OK:09037045,SYS:sub_list,EMPTY0,/WDG/EXPIRED,PUSH,count=0 []
```

```text
AT+PUB=SYS,TMR,0x0903703A,LIST,ALL
[14:39:19.879] [00000] AT Command: AT+PUB=SYS,TMR,0x0903703A,LIST,ALL
+DATA:0903703A,SYS:tmr,activeCount=0,poolCount=0,capacity=10,[]
```

```text
/log?-80
+OK
[14:39:27.668] [00000] Command received: /log?-80, Method: GET
~[14:37:14.075] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:14.094] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:14.107] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=3
~[14:37:15.188] [TRACE] SYS:publish_entry ctx=09037033 dest=SYS cmd=TMR args=INFO,sys1103_per sync=True fanout=False
~[14:37:15.196] [TRACE] SYS:publish_match ctx=09037033 dest=SYS cmd=TMR module=SYS
~[14:37:15.206] [TRACE] SYS:route_entry ctx=09037033 cmd=TMR args=INFO,sys1103_per
~[14:37:15.218] [TRACE] SYS:route_ok ctx=09037033 cmd=TMR result=+DATA:09037033,SYS:tmr,id=sys1103_per,state=active,delayMs=100,type=TMR,consumer=,remainingMs=1850,tag=probe,context=09037032,mode=PERIODIC,periodMs=3000,delivering=false
~[14:37:17.072] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:17.091] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:17.104] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=4
~[14:37:20.073] [TRACE] SYS:emit_event ctx=09037032 topic=SYS/TMR/EXPIRED
~[14:37:20.092] [TRACE] EMPTY0:deliver_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:20.105] [TRACE] EMPTY0:handler_event ctx=09037032 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=5
~[14:37:21.121] [TRACE] SYS:publish_entry ctx=09037034 dest=SYS cmd=TMR args=CANCEL,sys1103_per sync=True fanout=False
~[14:37:21.129] [TRACE] SYS:publish_match ctx=09037034 dest=SYS cmd=TMR module=SYS
~[14:37:21.139] [TRACE] SYS:route_entry ctx=09037034 cmd=TMR args=CANCEL,sys1103_per
~[14:37:21.151] [TRACE] SYS:route_ok ctx=09037034 cmd=TMR result=+OK:09037034,SYS:tmr,cancel,sys1103_per
~[14:37:46.239] [TRACE] SYS:publish_entry ctx=09037035 dest=SYS cmd=TMR args=START,sys1103_per sync=True fanout=False
~[14:37:46.248] [TRACE] SYS:publish_match ctx=09037035 dest=SYS cmd=TMR module=SYS
~[14:37:46.258] [TRACE] SYS:route_entry ctx=09037035 cmd=TMR args=START,sys1103_per
~[14:37:46.269] [TRACE] SYS:route_ok ctx=09037035 cmd=TMR result=+OK:09037035,SYS:tmr,start,sys1103_per
~[14:37:46.703] [TRACE] SYS:emit_event ctx=09037035 topic=SYS/TMR/EXPIRED
~[14:37:46.722] [TRACE] EMPTY0:deliver_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:46.735] [TRACE] EMPTY0:handler_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=1
~[14:37:49.372] [TRACE] SYS:emit_event ctx=09037035 topic=SYS/TMR/EXPIRED
~[14:37:49.391] [TRACE] EMPTY0:deliver_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:49.404] [TRACE] EMPTY0:handler_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=2
~[14:37:52.374] [TRACE] SYS:emit_event ctx=09037035 topic=SYS/TMR/EXPIRED
~[14:37:52.393] [TRACE] EMPTY0:deliver_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:37:52.406] [TRACE] EMPTY0:handler_event ctx=09037035 topic=SYS/TMR/EXPIRED filter='$arg0=sys1103_per' args=sys1103_per,probe,mode=PER,count=3
~[14:37:55.719] [TRACE] SYS:publish_entry ctx=09037036 dest=SYS cmd=TMR args=CANCEL,sys1103_per sync=True fanout=False
~[14:37:55.727] [TRACE] SYS:publish_match ctx=09037036 dest=SYS cmd=TMR module=SYS
~[14:37:55.737] [TRACE] SYS:route_entry ctx=09037036 cmd=TMR args=CANCEL,sys1103_per
~[14:37:55.748] [TRACE] SYS:route_ok ctx=09037036 cmd=TMR result=+OK:09037036,SYS:tmr,cancel,sys1103_per
~[14:38:17.628] [TRACE] SYS:publish_entry ctx=09037037 dest=SYS cmd=TMR args=CLOSE,sys1103_per sync=True fanout=False
~[14:38:17.637] [TRACE] SYS:publish_match ctx=09037037 dest=SYS cmd=TMR module=SYS
~[14:38:17.647] [TRACE] SYS:route_entry ctx=09037037 cmd=TMR args=CLOSE,sys1103_per
~[14:38:17.658] [TRACE] SYS:route_ok ctx=09037037 cmd=TMR result=+OK:09037037,SYS:tmr,close,sys1103_per
~[14:38:25.208] [TRACE] SYS:publish_entry ctx=09037038 dest=SYS cmd=UNSUB args=EMPTY0,/TMR/EXPIRED,PUSH sync=True fanout=False
~[14:38:25.216] [TRACE] SYS:publish_match ctx=09037038 dest=SYS cmd=UNSUB module=SYS
~[14:38:25.226] [TRACE] SYS:route_entry ctx=09037038 cmd=UNSUB args=EMPTY0,/TMR/EXPIRED,PUSH
~[14:38:25.241] [TRACE] SYS:crosssub_remove ctx=09037038 consumer=EMPTY0 topic=/TMR/EXPIRED filter='$arg0=sys1103_per' mode=PUSH
~[14:38:25.254] [TRACE] SYS:route_default ctx=09037038 cmd=UNSUB result=+OK:09037038,SYS:unsub,EMPTY0,/TMR/EXPIRED,PUSH,removed=1
~[14:38:33.201] [TRACE] SYS:publish_entry ctx=09037039 dest=SYS cmd=TMR args=OPEN,sys1103_bad,100,probe,0 sync=True fanout=False
~[14:38:33.209] [TRACE] SYS:publish_match ctx=09037039 dest=SYS cmd=TMR module=SYS
~[14:38:33.219] [TRACE] SYS:route_entry ctx=09037039 cmd=TMR args=OPEN,sys1103_bad,100,probe,0
~[14:38:33.231] [TRACE] SYS:route_ok ctx=09037039 cmd=TMR result=+ERR:09037039,SYS,ERANGE
~[14:38:40.373] [TRACE] SYS:publish_entry ctx=09037040 dest=SYS cmd=TMR args=WDG,sys1103_wdg,60000,EMPTY0,probe,ACK,act sync=True fanout=False
~[14:38:40.383] [TRACE] SYS:publish_match ctx=09037040 dest=SYS cmd=TMR module=SYS
~[14:38:40.394] [TRACE] SYS:route_entry ctx=09037040 cmd=TMR args=WDG,sys1103_wdg,60000,EMPTY0,probe,ACK,act
~[14:38:40.411] [TRACE] SYS:crosssub_list ctx=09037040 consumer=EMPTY0 topic=/WDG/EXPIRED mode=PUSH count=0 []
~[14:38:40.430] [TRACE] SYS:crosssub_add ctx=09037040 consumer=EMPTY0 topic=/WDG/EXPIRED filter='$arg2=EMPTY0' mode=PUSH
~[14:38:40.445] [TRACE] SYS:route_ok ctx=09037040 cmd=TMR result=+OK:09037040,SYS:tmr,wdg,sys1103_wdg
~[14:38:47.233] [TRACE] SYS:publish_entry ctx=09037041 dest=SYS cmd=TMR args=WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,BAD sync=True fanout=False
~[14:38:47.242] [TRACE] SYS:publish_match ctx=09037041 dest=SYS cmd=TMR module=SYS
~[14:38:47.256] [TRACE] SYS:route_entry ctx=09037041 cmd=TMR args=WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,BAD
~[14:38:47.268] [TRACE] SYS:route_ok ctx=09037041 cmd=TMR result=+DATA:09037041,SYS:tmr,wdg_confirm,sys1103_wdg,ignored,reason=reply_mismatch
~[14:38:53.374] [TRACE] SYS:publish_entry ctx=09037042 dest=SYS cmd=TMR args=WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK sync=True fanout=False
~[14:38:53.383] [TRACE] SYS:publish_match ctx=09037042 dest=SYS cmd=TMR module=SYS
~[14:38:53.394] [TRACE] SYS:route_entry ctx=09037042 cmd=TMR args=WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK
~[14:38:53.413] [TRACE] SYS:crosssub_remove ctx=09037042 consumer=EMPTY0 topic=/WDG/EXPIRED filter='$arg2=EMPTY0' mode=PUSH
~[14:38:53.427] [TRACE] SYS:route_ok ctx=09037042 cmd=TMR result=+DATA:09037042,SYS:tmr,wdg_confirm,sys1103_wdg,accepted
~[14:39:00.236] [TRACE] SYS:publish_entry ctx=09037043 dest=SYS cmd=TMR args=WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK sync=True fanout=False
~[14:39:00.245] [TRACE] SYS:publish_match ctx=09037043 dest=SYS cmd=TMR module=SYS
~[14:39:00.255] [TRACE] SYS:route_entry ctx=09037043 cmd=TMR args=WDG_CONFIRM,sys1103_wdg,0x09037040,EMPTY0,act,ACK
~[14:39:00.267] [TRACE] SYS:route_ok ctx=09037043 cmd=TMR result=+DATA:09037043,SYS:tmr,wdg_confirm,sys1103_wdg,ignored,reason=not_pending
~[14:39:07.996] [TRACE] SYS:publish_entry ctx=09037044 dest=SYS cmd=TMR args=CLOSE,sys1103_wdg sync=True fanout=False
~[14:39:08.020] [TRACE] SYS:publish_match ctx=09037044 dest=SYS cmd=TMR module=SYS
~[14:39:08.029] [TRACE] SYS:route_entry ctx=09037044 cmd=TMR args=CLOSE,sys1103_wdg
~[14:39:08.055] [TRACE] SYS:route_ok ctx=09037044 cmd=TMR result=+OK:09037044,SYS:tmr,close,sys1103_wdg
~[14:39:14.259] [TRACE] SYS:publish_entry ctx=09037045 dest=SYS cmd=SUBLIST args=EMPTY0,/WDG/EXPIRED,PUSH sync=True fanout=False
~[14:39:14.267] [TRACE] SYS:publish_match ctx=09037045 dest=SYS cmd=SUBLIST module=SYS
~[14:39:14.277] [TRACE] SYS:route_entry ctx=09037045 cmd=SUBLIST args=EMPTY0,/WDG/EXPIRED,PUSH
~[14:39:14.293] [TRACE] SYS:crosssub_list ctx=09037045 consumer=EMPTY0 topic=/WDG/EXPIRED mode=PUSH count=0 []
~[14:39:14.303] [TRACE] SYS:route_default ctx=09037045 cmd=SUBLIST result=+OK:09037045,SYS:sub_list,EMPTY0,/WDG/EXPIRED,PUSH,count=0 []
~[14:39:19.912] [TRACE] SYS:publish_entry ctx=0903703A dest=SYS cmd=TMR args=LIST,ALL sync=True fanout=False
~[14:39:19.921] [TRACE] SYS:publish_match ctx=0903703A dest=SYS cmd=TMR module=SYS
~[14:39:19.930] [TRACE] SYS:route_entry ctx=0903703A cmd=TMR args=LIST,ALL
~[14:39:19.943] [TRACE] SYS:route_ok ctx=0903703A cmd=TMR result=+DATA:0903703A,SYS:tmr,activeCount=0,poolCount=0,capacity=10,[]

```

```text
/log?filter=
+OK
[14:39:40.756] [00000] Command received: /log?filter=, Method: GET
+OK: filter=(null)
```

```text
/log?info
+OK
[14:39:49.354] [00000] Command received: /log?info, Method: GET
+OK: loginfo[size=80, head=18, tail=16, index=17, count=79, filter=(null), trigger=(null), post=0]
```
