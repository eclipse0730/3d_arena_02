# 3D Arena Survival Development Progress

## 1. 문서 목적

이 문서는 실제 개발 진행 내역을 누적 기록하는 작업 로그다.
앞으로 구현 단계가 끝날 때마다 아래 기록에 계속 추가한다.

## 2. 기록 규칙

- 날짜 기준으로 작업 내용을 순서대로 남긴다.
- 구현 내용, 확인 방법, 다음 작업을 함께 적는다.
- 초보자가 다시 읽어도 흐름이 보이도록 쉬운 표현을 사용한다.

## 3. 진행 기록

### 2026-03-22 - 개발 흐름 정리와 1단계 시작

작업 내용:
- `PRD.md`를 읽고 전체 게임 방향을 정리했다.
- PRD를 실제 구현 순서로 바꾼 개발 가이드 문서 `DevelopmentPlan.md`를 만들었다.
- 메인 씬에 `GameRoot`, `Managers`, `Arena`, `Players`, `UI` 구조를 추가했다.
- 관전용 카메라 위치를 조정했다.
- 기본 바닥과 초기 매니저 스크립트 자리를 만들었다.

추가된 주요 파일:
- `Assets/Docs/DevelopmentPlan.md`
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 C# 스크립트 컴파일 확인
- 경고 0개, 오류 0개 상태까지 정리

현재 상태:
- 씬의 기본 뼈대가 준비된 상태
- 다음으로 경기장 타일 생성을 붙이기 좋은 상태

다음 작업:
- `ArenaManager`에서 타일형 경기장을 자동 생성하도록 구현

### 2026-03-22 - 2단계 경기장 생성 구현

작업 내용:
- `ArenaManager`에 타일 맵 자동 생성 기능을 추가했다.
- 경기장 크기(`width`, `height`)와 타일 크기(`tileSize`, `tileHeight`, `tileGap`)를 인스펙터에서 조절할 수 있게 했다.
- `Arena` 오브젝트 아래에 `Tiles` 루트를 만들고 그 안에 타일 큐브들을 자동 생성하도록 구성했다.
- 각 타일의 월드 위치를 계산하는 함수와 랜덤 스폰 위치를 얻는 함수를 추가했다.
- 기존 `Ground` 오브젝트가 경기장 크기에 맞게 자동으로 넓어지도록 연결했다.

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 다시 컴파일 확인
- 경고 0개, 오류 0개

현재 상태:
- 씬을 열면 `ArenaManager`가 타일 경기장을 자동으로 준비할 수 있는 상태
- 다음 단계에서 이 타일 위에 플레이어를 생성할 기반이 준비됨

다음 작업:
- 플레이어 프리팹 1종 만들기
- 플레이어 여러 명 생성 기능 붙이기

### 2026-03-22 - 3단계 플레이어 기본형과 4단계 여러 명 생성 시작

작업 내용:
- `PlayerController`를 추가해 플레이어 기본형의 물리 설정을 한 곳에 모았다.
- 플레이어가 `Rigidbody`, `CapsuleCollider`를 갖고 항상 서 있는 형태를 유지하도록 기본값을 넣었다.
- `GameManager`가 플레이 모드 시작 시 여러 명의 참가자를 자동 생성하도록 확장했다.
- 참가자 수, 기본 이름 목록, 색상 팔레트를 인스펙터에서 조절할 수 있게 했다.
- 경기장 타일 좌표를 섞어서 서로 다른 위치에 플레이어가 배치되도록 구성했다.

추가된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 다시 컴파일 확인
- 경고 0개, 오류 0개

현재 상태:
- 플레이 모드 시작 시 캡슐 형태의 플레이어가 여러 명 자동 생성될 수 있는 상태
- 캐릭터마다 기본 이름과 색상이 들어가는 상태
- 아직 자동 이동과 충돌 넉백은 붙지 않은 상태

다음 작업:
- 플레이어 AI 자동 이동 추가
- 충돌 넉백과 파워 값 연결

### 2026-03-22 - 5단계 AI 자동 이동 추가

작업 내용:
- `ArenaManager`에 경기장 중심 위치와 범위 판정 helper를 추가했다.
- `PlayerController`에 기본 자동 이동 로직을 추가했다.
- 플레이어가 일정 시간마다 방향을 바꾸며 움직이도록 만들었다.
- 캐릭터가 바깥으로 치우치면 경기장 중심 쪽으로 다시 돌아오도록 보정했다.
- 이동은 `Rigidbody` 기반으로 처리해서 이후 충돌 넉백 단계와 자연스럽게 이어질 수 있게 했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 다시 컴파일 확인
- 경고 0개, 오류 0개

현재 상태:
- 플레이 모드 시작 시 여러 플레이어가 경기장 위에 생성된다.
- 생성된 플레이어가 경기장 안에서 자동으로 움직인다.
- 아직 충돌 넉백, 탈락, 결과 판정은 붙지 않은 상태

다음 작업:
- 충돌 넉백 추가
- 낙하 탈락과 생존 판정 추가

### 2026-03-22 - 충돌 넉백과 기본 탈락 판정 추가

작업 내용:
- `PlayerController`에 플레이어 간 충돌 시 넉백이 발생하는 로직을 추가했다.
- 넉백 강도가 각 플레이어의 `power` 값 영향을 받도록 연결했다.
- 플레이어가 일정 높이 아래로 떨어지면 탈락 처리되도록 추가했다.
- `GameManager`가 탈락 순서를 기록하고 마지막 생존자를 우승자로 판정하도록 확장했다.
- 기본 `Ground`는 시각용 받침으로 두고, 실제 발판 역할은 타일이 맡도록 콜라이더를 껐다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 다시 컴파일 확인
- 경고 0개, 오류 0개

현재 상태:
- 플레이어가 자동 생성되고 경기장 안에서 움직인다.
- 충돌 시 서로 밀려난다.
- 타일 밖으로 밀리거나 떨어지면 탈락할 수 있다.
- 마지막 1명이 남으면 내부적으로 우승자 판정이 가능하다.

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 붕괴 연출 강화와 Shrink UI 추가

작업 내용:
- 플레이 중에도 프리팹 자산에 대해 `Renderer.material` 접근이 일어나지 않도록 `PlayerController.ApplyVisuals()`를 다시 보강했다.
- `PlayerPrefabAutoBuilder`가 플레이 중에는 프리팹 자산을 확인/생성하지 않도록 수정해서 불필요한 에디터 접근을 막았다.
- 경기장 붕괴 속도와 간격을 더 극적으로 조정했다.
- 외곽 붕괴 전에 내부 타일이 한 칸씩 랜덤으로 먼저 무너지는 패턴을 추가했다.
- `UIManager`가 자동으로 Canvas와 텍스트를 준비하고, 전투 중 `Shrink In: x.x` 카운트다운을 표시하도록 확장했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Scripts/Editor/PlayerPrefabAutoBuilder.cs`
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개, 오류 0개

현재 상태:
- 프리팹 머티리얼 접근 에러가 다시 나올 가능성을 줄인 상태
- 경기장 붕괴가 더 빠르고 드라마틱하게 보일 수 있는 상태
- 랜덤 단일 타일 붕괴와 다음 붕괴 카운트다운 UI가 동작할 수 있는 상태

다음 작업:
- 결과 UI 본격 구성
- 우승자/순위 표시 연결

### 2026-03-22 - 붕괴 시작 딜레이 조정

작업 내용:
- 내부 랜덤 타일 붕괴 시작 딜레이를 `2.5초`에서 `7.5초`로 늘렸다.
- 외곽 링 붕괴 시작 딜레이를 `5초`에서 `10초`로 늘렸다.
- 붕괴 간격 자체는 유지하고, 시작 시점만 더 늦춰서 초반 관전 시간이 조금 더 확보되도록 조정했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`

현재 상태:
- 라운드 시작 후 약 7.5초부터 내부 랜덤 붕괴 시작
- 라운드 시작 후 약 10초부터 외곽 링 붕괴 시작

### 2026-03-22 - 붕괴 간격 딜레이 추가

작업 내용:
- 내부 랜덤 타일 붕괴 사이 간격을 `0.55초`에서 `5.55초`로 늘렸다.
- 외곽 링 붕괴 사이 간격을 `0.8초`에서 `5.8초`로 늘렸다.
- 시작 시점뿐 아니라 각 붕괴 이벤트 사이에도 충분한 관전 템포가 생기도록 조정했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`

현재 상태:
- 내부 랜덤 붕괴는 약 5.55초 간격
- 외곽 링 붕괴는 약 5.8초 간격

### 2026-03-22 - 에디터 리로드 안정화

작업 내용:
- 스크립트 리로드 때 자동으로 프리팹을 생성하던 `PlayerPrefabAutoBuilder`의 `InitializeOnLoad` 동작을 제거했다.
- 프리팹 생성은 이제 메뉴 `Tools/3D Arena/Ensure Player Prefab`으로만 수동 실행되게 바꿨다.
- `SceneSetupBootstrap`, `ArenaManager`, `UIManager`에서 `ExecuteAlways` 기반 편집 모드 자동 변경을 제거했다.
- `OnValidate`와 `DestroyImmediate` 중심의 에디터 타이밍 씬 변경을 줄이고, 대부분의 초기화가 Play 중에만 일어나도록 정리했다.
- 이 변경으로 스크립트 업데이트 시 에디터가 씬/프리팹/캔버스를 동시에 다시 쓰는 위험을 크게 줄였다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Editor/PlayerPrefabAutoBuilder.cs`
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개, 오류 0개

현재 상태:
- 에디터 스크립트 리로드 시 자동 자산/씬 변형이 크게 줄어든 상태
- 튕김 원인으로 의심되던 위험 패턴을 제거한 상태

### 2026-03-22 - 경기장 바깥 링 자동 붕괴 추가

작업 내용:
- 전투 시작 후 5초가 지나면 경기장 바깥 링이 하나씩 무너지도록 `ArenaManager`에 타이머 기반 축소 로직을 추가했다.
- 축소는 외곽 한 겹씩 진행되며, 각 타일은 콜라이더가 꺼진 뒤 아래로 떨어지는 연출을 가진다.
- 경기장 최소 크기는 `5 x 5`로 제한해서 그 이하로는 더 줄어들지 않도록 했다.
- 현재 활성 경기장 크기에 맞춰 외곽 판정도 함께 줄어들도록 반영했다.
- `GameManager`가 라운드 시작 시 축소 타이머를 시작하고, 라운드 종료 시 축소를 멈추도록 연결했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

현재 상태:
- 게임 시작 후 5초 뒤부터 경기장 바깥쪽 타일이 한 겹씩 무너질 수 있는 상태
- 최소 경기장 크기는 5 x 5 상태

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 플레이어 스폰 참조 꼬임 수정

작업 내용:
- `GameManager.playersRoot`가 씬의 `Players` 루트가 아니라 외부 프리팹 자산을 잘못 참조하던 문제를 수정했다.
- `TryResolveReferences()`가 이제 씬 오브젝트가 아닌 잘못된 참조를 감지하면 자동으로 `GameRoot/Players`를 다시 찾도록 보강했다.
- 플레이어 생성도 `Instantiate(prefab, position, rotation, parent)` 형태로 바꿔서 월드 위치 적용이 더 명확하게 되도록 정리했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

현재 상태:
- 플레이어가 중앙 한 점에 몰려 생성되던 참조 꼬임 문제를 코드에서 자동 복구할 수 있는 상태

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 프리팹 시각 적용 에러 수정

작업 내용:
- `PlayerController.OnValidate()`에서 프리팹 에셋 상태의 `Renderer.material`에 접근하던 문제를 수정했다.
- 프리팹 에셋에서는 `sharedMaterial`을 사용하고, 플레이 중 인스턴스에서는 `material`을 사용하는 분기 로직을 추가했다.
- 렌더러나 물리 컴포넌트가 아직 준비되지 않은 순간에도 `NullReferenceException`이 나지 않도록 방어 코드를 추가했다.
- 머티리얼이 없는 경우를 대비해 기본 URP Lit 머티리얼을 자동으로 만들어 연결하도록 처리했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개, 오류 0개

현재 상태:
- 플레이어 프리팹을 Project 창에서 다룰 때 `Renderer.material` 관련 에러가 나지 않는 상태
- `OnValidate()` 중 시각 갱신이 더 안전하게 동작하는 상태

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 플레이어 프리팹 기반 구조로 리팩터링

작업 내용:
- `GameManager`가 더 이상 캡슐 프리미티브를 직접 만들지 않고 `Player` 프리팹을 스폰하도록 구조를 바꿨다.
- `playerPrefab` 필드를 추가해서 인스펙터 또는 `Resources` 경로에서 플레이어 프리팹을 참조할 수 있게 했다.
- 에디터 전용 `PlayerPrefabAutoBuilder`를 추가해서 `Assets/_Project/Resources/Prefabs/Player.prefab`이 없으면 자동으로 생성되도록 만들었다.
- 플레이어 프리팹 생성 로직은 캡슐 메시, 콜라이더, 리지드바디, `PlayerController`가 들어간 기본형을 기준으로 구성했다.
- 메뉴 `Tools/3D Arena/Ensure Player Prefab`로 수동 생성도 가능하도록 했다.

추가된 주요 파일:
- `Assets/_Project/Scripts/Editor/PlayerPrefabAutoBuilder.cs`

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개, 오류 0개

현재 상태:
- 런타임 플레이어 생성 구조가 프리팹 기반으로 전환된 상태
- Unity 에디터가 스크립트를 다시 불러오면 플레이어 프리팹이 자동 생성될 수 있는 상태
- 플레이어 외형과 컴포넌트를 이제 프리팹 기준으로 확장하기 쉬운 상태

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 중간 점검 2 수정

작업 내용:
- 레거시 씬에서 `ArenaManager` 크기가 10x10으로 남아 있으면 자동으로 15x15로 올려주도록 보정 로직을 추가했다.
- 더 이상 필요하지 않은 `Ground` 오브젝트는 부트스트랩에서 자동 제거하도록 바꿨다.
- 플레이어가 타일 바깥으로 밀린 뒤 끝에 걸쳐 버티는 문제를 줄이기 위해, 경기장 밖 판정을 시간 기반으로 추가했다.
- 외곽에서 방향이 자주 뒤집히며 버벅이던 문제를 줄이기 위해 이동 로직을 즉시 속도 덮어쓰기 방식에서 가속 기반 보간 방식으로 바꿨다.
- 플레이어 이동, 넉백, 중심 복귀, 탈락 판정 관련 수치를 `GameManager` 인스펙터에서 조절할 수 있도록 `PlayerRuntimeSettings`를 묶어 추가했다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 다시 컴파일 확인
- 경고 0개, 오류 0개

현재 상태:
- 기존 10x10 경기장은 열 때 15x15로 자동 업그레이드될 수 있는 상태
- 레거시 그라운드는 제거되는 상태
- 플레이어는 타일 밖에서 오래 버티지 못하고 탈락 판정으로 넘어갈 수 있는 상태
- 플레이어 움직임 관련 핵심 수치는 `GameManager`에서 조절 가능한 상태

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 중간 점검 수정

작업 내용:
- 플레이어가 바닥에 반쯤 파묻혀 보이던 문제를 수정했다.
- 원인은 캡슐 메시 중심과 콜라이더 중심이 다르게 잡혀 있던 점이었고, 콜라이더 중심을 원점으로 맞췄다.
- 타일 좌표 함수도 정리해서 스폰 위치가 타일 윗면 기준으로 계산되도록 수정했다.
- 충돌 시 밀려나지 않고 비비기만 하던 문제를 줄이기 위해 넉백 강도를 높였다.
- 충돌 직후 짧은 시간 동안 자동 이동이 넉백을 덮어쓰지 않도록 recovery 시간을 추가했다.
- 기본 맵 크기를 `15 x 15`로 올렸다.

수정된 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj`로 다시 컴파일 확인
- 경고 0개, 오류 0개

현재 상태:
- 플레이어 스폰 높이가 이전보다 자연스럽게 맞춰진 상태
- 충돌 시 밀려나는 반응이 더 잘 보이도록 조정된 상태
- 기본 경기장은 15 x 15 크기 상태

다음 작업:
- 결과 UI 추가
- 카운트다운과 경기 상태 표시 추가

### 2026-03-22 - 내부 타일 붕괴 제거

작업 내용:
- 내부에서 랜덤하게 타일이 먼저 무너지던 기능을 제거했다.
- 이제 타일 붕괴는 외곽 링이 시간차를 두고 한 겹씩 내려가는 방식만 유지된다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인 예정

현재 상태:
- 경기 시작 후 내부 타일은 유지된다.
- 붕괴는 바깥쪽부터 최소 5x5까지 진행된다.

### 2026-03-22 - 붕괴 예고 및 경기 UI 추가

작업 내용:
- 외곽 붕괴 직전의 타일이 붉은색으로 바뀌도록 경고 연출을 추가했다.
- 라운드 시작 전 중앙 카운트다운을 추가하고, 카운트다운 동안 플레이어 자동 이동이 잠시 멈추도록 정리했다.
- 좌상단에 경기 상태 표시를 추가해서 Setup, Countdown, Battle, Results를 확인할 수 있게 했다.
- 결과 패널을 추가해서 우승자와 최종 순위를 화면에서 바로 볼 수 있게 했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

현재 상태:
- 붕괴 직전 외곽 링은 붉게 표시된다.
- 게임 시작 시 중앙 카운트다운 후 전투가 시작된다.
- 좌상단에서 현재 경기 상태와 다음 붕괴 시간을 확인할 수 있다.
- 경기 종료 후 결과 패널에 우승자와 순위가 표시된다.

### 2026-03-22 - UI 표시 문제 수정

작업 내용:
- UI가 보이지 않던 문제를 해결하기 위해 `UI` 오브젝트 아래에 전용 `RuntimeCanvas`를 만들고, 모든 텍스트와 결과 패널이 그 아래에 생성되도록 수정했다.
- CanvasScaler와 GraphicRaycaster도 전용 캔버스 기준으로 붙도록 정리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - UI 강제 초기화 보강

작업 내용:
- `UIManager`가 전용 캔버스를 직접 초기화하는 공개 메서드를 추가했다.
- `SceneSetupBootstrap`과 `GameManager`에서 UI 초기화를 한 번 더 호출해서, Play 시작 시 UI가 반드시 생성되도록 보강했다.
- 런타임에 생성되는 UI 오브젝트를 `RectTransform` 기반으로 만들도록 수정했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - UI 내장 폰트 오류 수정

작업 내용:
- Unity 내장 폰트 경로를 `Arial.ttf`에서 `LegacyRuntime.ttf` 우선 사용으로 변경했다.
- 폰트를 찾지 못하는 경우를 대비해 기존 경로도 폴백으로 유지했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 넉백 튜닝 및 플레이어 상단 정보 추가

작업 내용:
- 충돌 시 위로 뜨는 힘을 조금 더 높였다.
- 맵 축소 후에도 플레이어가 예전 경기장 크기를 기준으로 움직이던 문제를 수정해서, 줄어든 경기장 크기를 기준으로 가장자리 회피가 작동하게 했다.
- 플레이어 머리 위에 이름과 파워 바가 보이도록 월드 스페이스 UI를 추가했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/MainScenes.unity`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

현재 상태:
- 충돌 시 캐릭터가 이전보다 조금 더 높게 튄다.
- 축소 이후 외곽 쪽으로 걸어 나가는 현상이 줄어든 상태다.
- 각 플레이어 위에 이름과 파워 바가 표시된다.

### 2026-03-22 - 씬 배치형 UI 전환 준비

작업 내용:
- `UIManager`에 씬에 배치된 UI 참조를 직접 바인딩하는 메서드를 추가했다.
- 에디터 전용 `SceneUIBuilder`를 추가해서 `MainScenes`가 열려 있을 때 `UI` 아래에 Canvas, 상태 텍스트, 축소 타이머, 카운트다운, 결과 패널을 실제 씬 오브젝트로 생성하고 연결하도록 했다.
- 메뉴 `Tools > 3D Arena > Ensure Scene UI`로 수동 생성도 가능하게 했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
- `Assets/_Project/Scripts/Editor/SceneUIBuilder.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 우승 확정 후 라운드 정지 수정

작업 내용:
- 우승자가 결정되면 추가 탈락 등록을 막도록 처리했다.
- 결과 진입 시 남아 있는 플레이어의 이동과 물리를 정지시키도록 수정했다.
- 결과 화면 진입 후 `Time.timeScale`을 0으로 내려서 라운드 전체가 멈추도록 했다.
- 새 라운드 시작이나 셋업 복귀 시에는 `Time.timeScale`을 다시 1로 복구한다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 씬 UI 인스펙터 값 유지 수정

작업 내용:
- `SceneUIBuilder`가 이미 존재하는 UI 오브젝트의 위치, 크기, 색상, 폰트 스타일을 다시 기본값으로 덮어쓰지 않도록 수정했다.
- `UIManager`도 씬에 바인딩된 UI 참조가 있으면 런타임에 새로 만들거나 레이아웃을 덮어쓰지 않도록 정리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Editor/SceneUIBuilder.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 동적 파워 게이지 추가

작업 내용:
- 플레이어 파워가 충돌 넉백 계산에 실제 반영되고 있음을 유지한 상태에서, 파워가 시간에 따라 주기적으로 강해졌다 약해졌다 하도록 수정했다.
- 각 플레이어마다 파워 진동 위상을 다르게 줘서 동시에 같은 타이밍으로 움직이지 않게 했다.
- 머리 위 파워 바가 현재 파워 값을 실시간으로 반영하도록 갱신 로직을 추가했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 인스펙터 정리

작업 내용:
- `PlayerRuntimeSettings`에 Movement, Collision, Power, Elimination 섹션을 나눠서 관련 값을 한눈에 조절할 수 있게 정리했다.
- `GameManager` 인스펙터도 Round, Participants, Scene References, Player Tuning, Spawn Power 구역으로 나눴다.
- 파워 관련 값은 `GameManager > Player Tuning > Power`에서 바로 조절할 수 있게 정리된 상태다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 충돌 파워 반영 방향 수정

작업 내용:
- 충돌 시 각 플레이어가 자기 파워만큼 자기 자신을 더 튕기던 문제를 수정했다.
- 이제 충돌 계산은 한 번만 처리되고, 내 파워는 상대를 더 멀리 밀어내는 방향으로 적용된다.
- 나는 상대 파워에 비례한 반작용만 받아서, 강한 플레이어가 상대를 더 강하게 밀어내는 체감이 나도록 정리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 외곽 붕괴 우수수 연출 추가

작업 내용:
- 외곽 링이 한 번에 통째로 떨어지지 않고, 타일마다 미세한 시차를 두고 순서대로 무너지도록 수정했다.
- `ringCollapseCascadeDelay` 값을 추가해서 인스펙터에서 우수수 떨어지는 간격을 조절할 수 있게 했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 외곽 붕괴 랜덤 순서 추가

작업 내용:
- 외곽 링 타일이 너무 규칙적으로 떨어지지 않도록, 같은 링 안에서 붕괴 순서를 랜덤하게 섞었다.
- 타일마다 작은 지연 편차를 더해서 우수수 무너지는 느낌이 더 자연스럽게 보이도록 조정했다.
- 인스펙터에서 `randomizeRingCollapseOrder`, `ringCollapseDelayJitter` 값을 조절할 수 있게 했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - 시작 화면 및 재시작 UI 추가

작업 내용:
- 시작 화면에 플레이어 수 표시, 이름 입력 목록, 인원 증감 버튼, 시작 버튼을 추가했다.
- 플레이어 수는 4명부터 20명까지 조절 가능하게 바꿨다.
- 이름은 `GameManager`에 유지되도록 해서, 게임 종료 후 다시 시작 화면으로 돌아와도 이전 값이 남도록 했다.
- 플레이어 수가 많아질수록 경기장 크기가 자동으로 커지도록 조정했다.
- 결과 패널 아래에 Restart 버튼을 추가해서 설정 화면으로 자연스럽게 돌아갈 수 있게 했다.
- 메인 씬은 자동 시작 대신 설정 화면이 먼저 뜨도록 변경했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
- `Assets/_Project/Scripts/Editor/SceneUIBuilder.cs`
- `Assets/_Project/MainScenes.unity`

확인 내용:
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 에디터 빌드에서 런타임/에디터 어셈블리 모두 통과
- 경고 0개 오류 0개

### 2026-03-22 - 셋업 UI 클릭 불가 수정

작업 내용:
- 클릭이 되지 않던 원인을 `EventSystem` 누락으로 확인하고, 씬 UI 빌더와 런타임 부트스트랩에서 `EventSystem`과 `StandaloneInputModule`을 자동 보장하도록 수정했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Editor/SceneUIBuilder.cs`
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개 오류 0개

### 2026-03-22 - Input System UI 모듈로 교체

작업 내용:
- 프로젝트가 `com.unity.inputsystem`을 사용 중이라 `StandaloneInputModule`이 예외를 내던 문제를 수정했다.
- 씬 UI 빌더와 런타임 부트스트랩 모두 `InputSystemUIInputModule`을 사용하도록 변경했다.
- 기존 `StandaloneInputModule`이 이미 붙어 있는 경우에는 제거 후 올바른 모듈로 교체하도록 처리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
- `Assets/_Project/Scripts/Editor/SceneUIBuilder.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개 오류 0개
### 2026-03-22 - 리빌드 안정성 점검

작업 내용:
- Unity 리빌드 시점의 불안정 원인을 코드와 `Editor.log` 기준으로 점검했다.
- 가장 큰 위험 요소로 보이던 에디터 리로드 시 자동 씬 변경 경로를 다시 확인했고, 현재 `SceneUIBuilder`가 메뉴 실행 방식만 사용하도록 유지되는 것을 확인했다.
- `PlayerController.OnValidate()`가 프리팹/컴포넌트 검증 중 과도한 셋업을 하지 않고, 참조 캐시와 비주얼 적용만 수행하도록 완화된 상태를 확인했다.
- `Editor.log` 마지막 구간에는 네이티브 크래시 스택은 없었고, 도메인 리로드와 복구 씬 로드가 반복되는 흔적이 확인됐다.

검토 결과:
- 현재 가장 의심되는 원인은 "스크립트 리로드 타이밍에 씬/프리팹을 자동으로 다시 만지는 에디터 코드"였고, 지금은 그 위험이 크게 줄어든 상태다.
- 다만 `Assets/_Recovery/` 씬이 계속 import/open 대상이 되고 있어, 에디터 복구 상태가 남아 있으면 체감상 더 불안정해질 수 있다.
- 런타임/에디터 어셈블리는 모두 정상 빌드된다.

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 경고 0개, 오류 0개

### 2026-03-22 - UIManager 씬 배치형 전용 정리

작업 내용:
- `UIManager`의 런타임 UI 생성 fallback을 제거했다.
- 이제 `UIManager`는 씬에 배치된 `Canvas`, HUD, SetupPanel, ResultsPanel 참조만 사용한다.
- 씬 참조가 비어 있으면 UI를 새로 만들지 않고, `Tools > 3D Arena > Ensure Scene UI` 사용 안내 경고만 출력하도록 정리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- `dotnet build Assembly-CSharp-Editor.csproj` 확인
- 런타임 빌드 오류 0개
- 에디터 빌드는 `Temp/obj` 상태 파일 잠금 경고 1개가 있었지만 출력 DLL 생성은 완료됐다.
### 2026-03-22 - Setup UI 입력 동작 복구

작업 내용:
- 메인 씬에 아직 `StandaloneInputModule`이 남아 있는 상태를 확인했다.
- 런타임 `EventSystem` 보정 로직이 `Destroy(StandaloneInputModule)` 직후 같은 프레임에 `BaseInputModule` 존재 여부를 확인하고 있어, 실제로 `InputSystemUIInputModule`이 추가되지 않을 수 있는 문제를 수정했다.
- 이제 런타임과 에디터 UI 빌더 모두 `InputSystemUIInputModule`를 명시적으로 보장하고, `actionsAsset`이 비어 있으면 `AssignDefaultActions()`를 호출해 기본 UI 입력 액션을 자동으로 채운다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`
- `Assets/_Project/Scripts/Editor/SceneUIBuilder.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

### 2026-03-22 - 이름 입력 UX 개선

작업 내용:
- Setup UI의 이름 입력칸에서 `Tab` 키를 누르면 다음 입력칸으로 포커스가 이동하도록 추가했다.
- 마우스 클릭이나 `Tab` 이동으로 입력칸이 활성화되면 기존 텍스트를 비우고 처음부터 바로 입력할 수 있게 수정했다.
- `InputField` 선택 이벤트는 `EventTrigger.Select`를 사용해 연결했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

### 2026-03-22 - 최소 인원 및 패널 정렬 개선

작업 내용:
- 참가 인원 하한을 `2명`으로 낮췄고, 현재 메인 씬 기본값도 `2명`으로 맞췄다.
- `GameManager`와 `UIManager`의 주요 `SerializeField`에 `Tooltip` 설명을 추가해 인스펙터에서 역할을 바로 확인할 수 있게 정리했다.
- 참가 인원이 많아질 때 SetupPanel과 ResultsPanel 크기가 커지도록 적응형 레이아웃을 추가했다.
- 참가 인원이 많을 때 Setup 이름 입력 행 높이와 폰트 크기를 줄여 화면 안에서 더 안정적으로 보이게 조정했다.
- 참가 인원이 많을 때 ResultsPanel의 크기와 텍스트 크기를 함께 조정해 순위 목록이 더 잘 보이도록 정리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
- `Assets/_Project/MainScenes.unity`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

### 2026-03-22 - Tooltip 한글화 및 2열 UI 정리

작업 내용:
- `GameManager`와 `UIManager`의 주요 `Tooltip` 문구를 한글로 바꿨다.
- 참가 인원 감소 버튼이 `4명` 아래로 내려가지 않던 문제를 수정해서 `2명`까지 정상 동작하도록 정리했다.
- 참가 인원이 `11명 이상`이면 Setup 이름 입력 목록이 2열로 배치되도록 레이아웃을 전환했다.
- 참가 인원이 `11명 이상`이면 Results 순위 목록도 2열로 배치되도록 동적 결과 레이아웃을 추가했다.
- 결과 패널 제목과 참가 인원 표기도 한글 표현으로 맞췄다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

### 2026-03-22 - 2열 레이아웃 충돌 수정

작업 내용:
- `PlayerListContent`와 `ResultsListContent`에 서로 다른 `LayoutGroup`을 런타임에 겹쳐 붙이려 하면서 발생하던 오류를 수정했다.
- 셋업 이름 입력 목록은 `세로 콘텐츠 루트 + 가로 행` 구조로 다시 구성해, `11명 이상`일 때 각 행에 2개씩 배치되도록 변경했다.
- 결과 패널 순위 목록도 같은 방식으로 다시 구성해, `11명 이상`일 때 2열 표시가 안정적으로 동작하도록 정리했다.
- 참가 인원 감소 하한은 다시 확인했고 `2명`까지 정상적으로 내려가도록 유지했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 확인
- 경고 0개, 오류 0개

### 2026-03-22 - 플레이어 외형 1차 적용

작업 내용:
- 현재 플레이어의 물리와 게임 로직은 유지한 채, 런타임에 외형만 `Sparrow` 모델로 붙이는 1차 적용을 진행했다.
- `PlayerController`에 캐릭터 외형 프리팹 참조와 위치/회전/스케일 값을 추가했다.
- 플레이 시작 시 `CharacterVisual` 자식으로 `Sparrow.prefab`을 생성하고, 외형 쪽 콜라이더는 비활성화하도록 정리했다.
- 기본 캡슐 메시 렌더러는 외형 프리팹이 있을 때 숨기도록 변경했다.
- `Player.prefab`이 `Assets/Quirky Series Ultimate/FREE/Prefabs/Sparrow.prefab`을 참조하도록 연결했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Resources/Prefabs/Player.prefab`

확인 내용:
- 코드 참조와 프리팹 연결은 완료했다.
- `dotnet build Assembly-CSharp.csproj`는 Unity가 `Temp/obj` 경로를 잠그고 있어 파일 접근 거부로 검증하지 못했다.

### 2026-03-22 - Sparrow 분홍색 머티리얼 보정

작업 내용:
- `Sparrow` 외형이 분홍색으로 보이던 원인을 확인했다.
- 텍스처는 `Assets/Quirky Series Ultimate/FREE/Textures/T_Sparrow.png`에 정상 포함되어 있었고, 문제는 `M_Sparrow.mat`가 `SoftSurface.shader`를 참조하는 점이었다.
- 현재 프로젝트에서 해당 셰이더가 정상적으로 표시되지 않아, 런타임에 `URP Lit` 또는 `Standard` 머티리얼을 새로 만들고 `T_Sparrow.png`를 적용하도록 보정 로직을 추가했다.
- `Player.prefab`에도 `characterVisualMainTexture` 참조를 연결했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Resources/Prefabs/Player.prefab`

확인 내용:
- 텍스처와 참조 연결은 확인했다.
- `dotnet build Assembly-CSharp.csproj`는 Unity가 `Temp/obj` 경로를 잠그고 있어 파일 접근 거부로 검증하지 못했다.

### 2026-03-22 - 플레이어별 외형 색상 반영

작업 내용:
- 플레이어마다 이미 설정되던 `tint` 값을 `Sparrow` 외형 머티리얼에도 적용하도록 연결했다.
- 이제 각 플레이어는 기존 `playerColors` 순서대로 서로 다른 색으로 보일 수 있다.
- 색상은 외형 머티리얼 생성 시와 플레이어 설정 갱신 시 모두 다시 적용되도록 정리했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`

### 2026-03-23 - PartyMonsterDuo 추가 플레이어 프리팹 연결

작업 내용:
- `Assets/PartyMonsterDuo` 폴더가 이미 프로젝트에 정상 임포트되어 있는 것을 확인하고, 해당 에셋을 재사용하는 방향으로 정리했다.
- 기본 `Player.prefab`은 Sparrow 외형을 유지하고, 추가 프리팹인 `Player 1.prefab`에 `PartyMonsterDuo/Prefab/P01.prefab` 외형을 연결했다.
- `Player 1.prefab`에 `DefaultPolyart.png` 텍스처 참조를 연결해 현재 런타임 머티리얼 보정 로직과 함께 바로 보이도록 맞췄다.
- PartyMonsterDuo 외형이 캡슐 충돌체와 크게 어긋나지 않도록 위치와 스케일을 보수적으로 조정했다.

수정한 주요 파일:
- `Assets/_Project/Resources/Prefabs/Player 1.prefab`

### 2026-03-23 - 플레이어 프리팹 에디터 미리보기 빌더 추가

작업 내용:
- `Player 1.prefab`을 GameManager에 연결했을 때도 에디터에서 캡슐처럼 보이던 원인을 정리했다.
- 현재 플레이어 외형은 런타임에 붙는 구조라서, 프리팹 자산 자체는 캡슐로 보일 수 있다.
- 이를 보완하기 위해 `Tools > 3D Arena > Rebuild Player Variant Prefabs` 메뉴를 추가했다.
- 이 메뉴는 `Player.prefab`과 `Player 1.prefab`을 다시 열어 각각 외형 프리팹을 `CharacterVisual` 자식으로 미리 구워 넣고, 루트 캡슐 렌더러를 꺼서 에디터에서도 외형이 바로 보이게 만든다.
- 기본 `Player.prefab`은 Sparrow, `Player 1.prefab`은 PartyMonsterDuo(P01) 기준으로 재생성된다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Editor/PlayerVariantPrefabBuilder.cs`

### 2026-03-23 - Player 2 프리팹 및 Free Burrow 외형 추가

작업 내용:
- `Assets/Free Burrow Cute Series` 에셋이 정상 임포트된 것을 확인했다.
- 새 프리팹 `Assets/_Project/Resources/Prefabs/Player 2.prefab`을 추가했다.
- `Player 2.prefab`은 `Free Burrow Cute Series/Prefabs/Free Burrow.prefab` 외형과 `Free Burrow.psd` 텍스처를 사용하도록 연결했다.
- 기존 프리팹 복제본 안에 남아 있을 수 있는 이전 미리보기 외형 때문에 잘못된 모델이 남는 문제를 막기 위해, `PlayerController`가 런타임 시작 시 기존 `CharacterVisual` 자식을 제거하고 현재 설정된 외형 프리팹으로 다시 생성하도록 보강했다.
- `Tools > 3D Arena > Rebuild Player Variant Prefabs` 메뉴가 `Player 2.prefab`까지 같이 재생성하도록 확장했다.

수정한 주요 파일:
- `Assets/_Project/Resources/Prefabs/Player 2.prefab`
- `Assets/_Project/Resources/Prefabs/Player 2.prefab.meta`
- `Assets/_Project/Scripts/Runtime/Players/PlayerController.cs`
- `Assets/_Project/Scripts/Editor/PlayerVariantPrefabBuilder.cs`

### 2026-03-25 - 경기장 타일 머티리얼 보정 및 대형 맵 카메라 조정

작업 내용:
- 경기장 타일이 Play 시 분홍색으로 보이던 원인을 확인했고, `ArenaManager`가 타일 프리팹의 기존 비URP 머티리얼을 그대로 쓰지 않도록 보정했다.
- 타일 생성 시 첫 렌더러의 텍스처를 기준으로 URP 호환 공유 머티리얼을 새로 만들고, 모든 arena 타일 렌더러에 같은 머티리얼을 적용하도록 정리했다.
- `GameManager`의 `playerColors` 기본 팔레트를 확장해 색상 5개를 추가했고, 기존 씬에 직렬화된 배열이 더 짧아도 자동으로 뒤에 색을 보강하도록 처리했다.
- 맵 크기가 `16x16` 이상일 때는 `SceneSetupBootstrap`이 카메라를 기본 위치보다 약간 더 뒤와 위에서 보도록 조정하게 만들었다.
- 라운드 시작 시 참가 인원에 따라 arena 크기를 다시 설정한 뒤, 그 크기에 맞춰 카메라 위치도 함께 재적용하도록 연결했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/ArenaManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Bootstrap/SceneSetupBootstrap.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 빌드가 경고 0개, 오류 0개로 통과했다.
- 코드상으로는 타일 머티리얼이 URP 기준으로 강제 보정되고, `16x16` 이상 대형 맵에서만 카메라 오프셋이 적용되도록 확인했다.

다음 작업:
- Unity Play 모드에서 대형 맵 인원수 기준으로 카메라 거리와 플레이어 색상 구분감이 실제 화면에서도 자연스러운지 한 번 더 점검한다.

### 2026-03-25 - 전투 중 탈락 순위 HUD 추가

작업 내용:
- 게임 도중 플레이어가 탈락하면 화면 오른쪽에 실시간으로 탈락 순위를 보여주는 HUD 패널을 `UIManager`가 런타임에 직접 생성하도록 추가했다.
- 결과 화면용 최종 순위와 별도로, 전투 중에는 `GameManager`의 현재 `eliminationOrder`를 기준으로 이미 탈락한 플레이어들의 등수를 계산해 표시하도록 정리했다.
- 탈락 순위 패널은 전투(`Battle`) 상태에서만 보이고, 아직 탈락자가 없으면 숨겨지도록 처리했다.
- 표시 내용은 가장 최근에 높은 순위를 확정한 탈락자부터 위쪽에 오도록 구성해, 예를 들어 3등이 먼저 보이고 그 아래에 4등, 5등이 이어지게 했다.

수정한 주요 파일:
- `Assets/_Project/Scripts/Runtime/Core/GameManager.cs`
- `Assets/_Project/Scripts/Runtime/Core/UIManager.cs`

확인 내용:
- `dotnet build Assembly-CSharp.csproj` 빌드가 경고 0개, 오류 0개로 통과했다.
- 코드상으로 전투 중에만 오른쪽 HUD 패널이 활성화되고, 탈락자가 늘어날수록 순위 문자열이 즉시 갱신되도록 연결된 것을 확인했다.

다음 작업:
- Unity Play 모드에서 실제 전투 중 HUD 위치와 줄 간격이 화면 우측에서 과하게 크거나 겹치지 않는지 한 번 더 확인한다.
