@echo off

RD /S /Q outputs

REM GameServer
echo ===== Generating Protos for Game Server =====
mkdir outputs

ProtoGenerator.exe -inpath protos -infiles protos/enum.proto -outpath outputs
ProtoGenerator.exe -inpath protos -infiles protos/err_code.proto -outpath outputs
ProtoGenerator.exe -inpath protos -infiles protos/struct.proto -outpath outputs
ProtoGenerator.exe -inpath protos -infiles protos/api.proto -outpath outputs

xcopy /e /y /i /f protos outputs\protocol

python remove_lines.py outputs/protocol gogoproto

protoc.exe --proto_path=outputs/protocol outputs\protocol\err_code.proto outputs\protocol\enum.proto outputs\protocol\struct.proto -o outputs\proto.bytes

del ..\League of Legends 2022\Assets\Scripts\Protos\*.cs > NUL

copy /y outputs\*.cs ..\League of Legends 2022\Assets\Scripts\Protos\ > NUL

RD /S /Q outputs

pause

echo ===== Complete =====

RD /S /Q outputs
echo ===== Complete =====

pause