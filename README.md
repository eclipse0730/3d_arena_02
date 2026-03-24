# 3D Arena Survival

Unity 기반의 3D 아레나 서바이벌 프로젝트입니다.
여러 플레이어가 같은 경기장에서 자동 이동과 충돌을 반복하고, 시간이 지나면 바깥 타일이 붕괴하면서 최후의 1인을 가리는 구조를 목표로 개발하고 있습니다.

## 현재 구현 범위

- 참가자 수에 따라 경기장 크기 자동 조정
- 플레이어 자동 생성, 색상 구분, 외형 적용
- 전투 시작 카운트다운
- 경기장 외곽 타일 순차 붕괴
- 전투 중 탈락 순위 HUD
- 결과 화면 표시

## 실행 방법

1. Unity에서 씬 `Assets/_Project/MainScenes.unity`를 엽니다.
2. `GameRoot` 아래의 `Managers`, `Arena`, `Players`, `UI` 구성이 있는지 확인합니다.
3. `Play`를 실행합니다.
4. Setup 화면에서 인원 수와 이름을 조정한 뒤 게임을 시작합니다.

## 문서 운영 원칙

이 프로젝트는 개발 전에 계획을 먼저 정리하고, 개발 중에는 변경 이력을 계속 기록하는 방식으로 진행합니다.

### `DevelopmentPlan.md`

- 개발 시작 전에 먼저 작성합니다.
- 개발 단계 플랜, 구현 순서, 단계별 목표, 완료 기준을 정리합니다.
- 큰 기능을 시작하기 전에 무엇을 어떤 순서로 만들지 정하는 기준 문서로 사용합니다.

위치:
- `Assets/Docs/DevelopmentPlan.md`

### `DevelopmentProgress.md`

- 개발 중 소스가 추가되거나 변경될 때마다 기록합니다.
- 작업 내용, 수정한 주요 파일, 확인 내용, 다음 작업을 함께 남깁니다.
- 현재까지 무엇이 바뀌었는지 빠르게 추적할 수 있도록 유지합니다.

위치:
- `Assets/Docs/DevelopmentProgress.md`

## 기본 작업 흐름

1. 개발 시작 전 `DevelopmentPlan.md`에 개발 단계 플랜을 먼저 생성합니다.
2. 구현을 진행하면서 소스 추가 또는 수정이 발생할 때마다 `DevelopmentProgress.md`에 기록합니다.
3. 작업이 끝나면 변경 사항, 확인 내용, 다음 작업이 문서에 반영되어 있는지 확인합니다.

## 주요 폴더 구조

- `Assets/Docs`
  PRD, 개발 계획, 개발 진행 기록 문서
- `Assets/_Project/MainScenes.unity`
  메인 플레이 씬
- `Assets/_Project/Scripts/Runtime`
  런타임 게임 로직
- `Assets/_Project/Scripts/Editor`
  에디터 보조 도구
- `Assets/_Project/Resources`
  프리팹, 외형, 런타임 로드 리소스

## 자주 보는 스크립트

- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
  씬 기본 구조, 카메라, 라이트, UI 초기 세팅
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
  참가자 수, 라운드 진행, 플레이어 생성, 맵 크기 조정
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
  경기장 타일 생성, 붕괴, 맵 비주얼 처리
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
  Setup UI, HUD, 결과 UI, 탈락 순위 UI
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
  플레이어 이동, 충돌, 탈락, 외형 적용

## 작업 규칙

- 새 기능을 시작하기 전에 먼저 `DevelopmentPlan.md`에서 단계와 방향을 확인합니다.
- 구현 중 소스 파일이 추가되거나 수정되면 `DevelopmentProgress.md`에 바로 기록합니다.
- 기존 자산을 쓰더라도 프로젝트 기준에 맞게 런타임 보정이 필요한지 확인합니다.
- UI나 프리팹 구조를 바꿨다면 실제 Play 모드에서 한 번 더 확인합니다.

## 검증 방법

기본 C# 컴파일 확인:

```powershell
dotnet build Assembly-CSharp.csproj
```

추가 확인 권장:

- Unity Play 모드에서 Setup -> Countdown -> Battle -> Results 흐름 확인
- 참가자 수를 바꿔 맵 크기와 카메라 위치가 기대대로 바뀌는지 확인
- 탈락 순위 HUD와 결과 화면 순위가 일치하는지 확인

## 현재 주의사항

- 프로젝트는 URP 기준으로 동작합니다.
- 일부 UI는 씬 참조를 기반으로 하되, 일부 보조 패널은 런타임에 생성됩니다.
- 큰 맵은 카메라가 자동으로 조금 더 뒤에서 보이도록 보정됩니다.
- 타일 프리팹 머티리얼은 런타임에 URP 호환 머티리얼로 보정될 수 있습니다.

## 관련 문서

- PRD: `Assets/Docs/PRD.md`
- 개발 계획: `Assets/Docs/DevelopmentPlan.md`
- 개발 진행 기록: `Assets/Docs/DevelopmentProgress.md`
