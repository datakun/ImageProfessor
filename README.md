# ImageProfessor
Image Processing Server; C# / C++ / .NET / OpenCV

### 1. 클라이언트의 요청을 처리하는 부분
- 클라이언트의 요청을 큐에 쌓는 부분
  - ASP.NET 웹페이지
    - https://msdn.microsoft.com/ko-kr/library/fddycb06(v=vs.100).aspx
  - WCF, http에서 파일 전송 예제
    - https://www.codeproject.com/articles/166763/wcf-streaming-upload-download-files-over-http
  - 메시지 큐
    - http://getakka.net/docs/Getting%20started
    - https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html
- 큐에서 메시지를 가져와 이미지 처리하는 곳에 결과를 요청하는 부분
  - 이미지 처리 서버에 소켓으로 이미지 송수신
    - https://www.codeproject.com/articles/461938/small-file-transfer-from-server-to-client
    - https://www.codeproject.com/articles/24017/file-transfer-using-socket-application-in-c-net-2
    - https://www.codeproject.com/articles/32633/sending-files-using-tcp
- 처리된 이미지 결과를 저장소에서 가져오는 부분

### 2. 이미지를 처리하는 부분
- 이미지 처리하는 로직 부분
  - http://opencv.org/
- 처리된 이미지 결과를 저장소에 저장하는 부분
  - SQLite3 ?
  
