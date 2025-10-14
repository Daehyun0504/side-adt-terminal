EmbedBox

임베디드 개발자를 위한 Windows용 유틸리티 앱입니다.
보드로 파일 전송(XMODEM)과 이미지→헤더 변환 기능, 간단 도움말을 제공합니다.
C# / .NET / WinForms 기반.

목차

주요 기능

화면 구성

요구 사항

빠른 시작

사용 방법

빌드 & 게시

MSI 설치파일 만들기(선택)

폴더 구조 예시

로그 & 설정

릴리스 배포 가이드

문제 해결

FAQ

라이선스

주요 기능

Download 패널: PC → 보드 데이터 전송

프로토콜: XMODEM

직렬 포트/보레이트 설정 후 전송

MakeData 패널: 이미지 파일을 헤더 형식으로 변환

옵션: CDNS_XSPI_HEADER, CDNS_XSPI_HEADER_BOOTCODE

Help 패널: 프로그램 사용법/옵션 요약 안내

화면 구성

panelDownload : 전송 기능

panelMakeData : 변환 기능

panelHelp : 가이드 & 문서 버튼(“Open Documentation”—추후 온라인 문서/PDF 연결 예정)

코드 포인트:

ShowPanel(Panel target)에서 필요한 패널만 Visible = true

InitializeHelpPanel()에 가이드 텍스트와 버튼 정의

요구 사항

OS: Windows 10/11 (x64)

IDE: Visual Studio 2022 이상(.NET 데스크톱 개발 워크로드)

런타임

Framework-dependent 게시: 대상 PC에 .NET Desktop Runtime 8.0+ 필요

Self-contained 게시: 별도 런타임 불필요

빠른 시작
git clone https://github.com/<your-account>/EmbedBox.git
cd EmbedBox
# Visual Studio로 .sln 열기 → F5 실행

사용 방법
1) Download

보드와 PC를 시리얼로 연결

COM 포트/보레이트 설정

Download 버튼 클릭 → XMODEM 전송

2) MakeData

원본 이미지 선택

변환 옵션 선택

CDNS_XSPI_HEADER

CDNS_XSPI_HEADER_BOOTCODE

Convert/Make 버튼 클릭 → 헤더 포함 바이너리 생성

3) Help

프로그램 사용법 요약 확인

Open Documentation: 추후 온라인 매뉴얼/PDF로 연결 예정

빌드 & 게시
Visual Studio(권장)

구성: Release | x64

프로젝트 > 게시

대상: 폴더

배포 모드:

Framework-dependent (작은 용량) 또는

Self-contained (런타임 포함)

대상 런타임: win-x64

게시 출력(예): bin\Release\net8.0-windows\publish\

CLI
# Framework-dependent (작은 용량)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

# Self-contained (런타임 포함)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true


중요: 프로젝트 PlatformTarget(x64/x86)과 -r(win-x64/win-x86)을 일치시키세요.

MSI 설치파일 만들기(선택)
A) Visual Studio Installer Projects(간단)

VS 확장: Microsoft Visual Studio Installer Projects 설치

Setup Project 추가 → Primary output 및 리소스(아이콘/README) 포함

제품명/제조사/버전/설치 경로/바로가기 설정

빌드 → *.msi 생성

B) WiX Toolset(고급)

사용자 정의 작업, 서비스, 사전 요구사항 등 고급 설치 구성에 적합

폴더 구조 예시
EmbedBox/
├─ src/
│  └─ EmbedBox/                 # .csproj, Program.cs, UI, Panels
├─ docs/                        # 스크린샷/문서
├─ tools/                       # 스크립트/패키징
├─ .gitignore                   # VisualStudio 템플릿 권장
├─ EmbedBox.sln
└─ README.md

로그 & 설정

설정 파일(예시)

사용자 범위: %AppData%\EmbedBox\config.json

앱 범위: EmbedBox.exe.config

로그(예시)

%LocalAppData%\EmbedBox\logs\ 아래 롤링 로그

레벨: Trace|Debug|Information|Warning|Error|Critical

예시 config.json:

{
  "Theme": "Light",
  "Language": "ko-KR",
  "LogLevel": "Information",
  "Serial": { "Port": "COM3", "Baudrate": 115200 }
}

릴리스 배포 가이드

Portable ZIP: 게시 폴더를 ZIP으로 압축해 업로드

MSI: 설치형 배포가 필요할 때 사용

GitHub Releases에 업로드하고 변경사항/체크섬을 함께 기록

문제 해결
1) 아이콘 파일 에러(CS7064)

“아이콘 파일을 여는 동안 오류… 파일을 찾을 수 없습니다”

프로젝트 > 속성 > 응용 프로그램 > 아이콘 경로 확인

프로젝트 내 상대경로(assets\app.ico) 사용 권장

파일을 프로젝트에 포함하고 필요 시 빌드 작업: Content

2) RuntimeIdentifier vs PlatformTarget 불일치

“RuntimeIdentifier 'win-x86' 과 PlatformTarget 'x64'은 호환돼야…”

PlatformTarget = x64 → -r win-x64

PlatformTarget = x86 → -r win-x86

VS 구성 관리자와 프로젝트 빌드 탭에서 확인/정렬

3) .gitignore가 동작 안 함

GitHub의 VisualStudio 템플릿 사용(bin/, obj/, .vs/, *.user 등 제외)

상태 확인: git status --ignored

4) 다른 PC에서 실행 안 됨

Framework-dependent 게시 시 대상 PC에 .NET Desktop Runtime 필요

호환성 문제 피하려면 Self-contained 게시 사용

FAQ

Q. Release/Debug 산출물을 커밋해야 하나요?
A. 아니요. 산출물은 커밋 대상이 아닙니다. 소스만 커밋하고, 배포 파일은 Releases에 올리세요.

Q. XMODEM 외 프로토콜도 지원하나요?
A. 현재 기본은 XMODEM입니다. 필요 시 옵션 확장 계획이 있습니다(기여 환영).

Q. 온라인 매뉴얼은 어디에 있나요?
A. Help 패널의 “Open Documentation”은 추후 온라인 문서 또는 PDF로 연결될 예정입니다.

라이선스
MIT License (영문 원문)
MIT License

Copyright (c) [YEAR] [YOUR NAME]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.