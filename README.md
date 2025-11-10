HappyFuture - C# 해외선물 자동매매 트레이딩 시스템
C# (.NET Framework, WinForms)로 개발된 해외선물 자동매매 봇입니다. 키움증권 API를 활용하여 실시간 데이터 수신, 전략 백테스팅, 자동 주문 실행 기능을 제공합니다.

📌 주요 기능

키움증권 API 연동: AxKFOpenAPI (COM) 컴포넌트를 C#으로 래핑하여 해외선물 시세 및 주문 기능을 구현합니다.

상태 머신 기반 운영: FutureState (ListUpFuturePhase, WatchingFuturePhase 등)를 통해 봇의 동작 상태를 명확하게 관리하여 안정성을 높였습니다.

전략 백테스팅: FutureBackTestRecoder를 이용해 과거 데이터를 기반으로 매매 전략의 유효성을 검증합니다.

실시간 자동 매매: FutureFundManagement 및 하위 전략 모듈을 통해 설정된 로직에 따라 자동으로 포지션에 진입하고 청산합니다.

데이터 영속성: SQLite 데이터베이스 (FuturePriceDB, FutureTradeTecodeDB)를 사용하여 분봉 데이터와 모든 매매 기록을 영속적으로 저장하고 관리합니다.

실시간 알림: 텔레그램 (Telegram) 봇과 연동하여 주문 체결, 오류 발생, 일일 손익 등 주요 이벤트를 실시간으로 사용자에게 알립니다.

🛠️ 주요 기술 스택

언어: C# (NET Framework, WinForms)

API: 키움증권 해외선물 API (COM)

데이터베이스: SQLite

알림: Telegram API

⚠️ 면책조항

본 소프트웨어는 실제 자금이 운용되는 자동매매 시스템입니다. 모든 매매의 최종 책임은 사용자에게 있습니다.

소프트웨어의 버그, 로직의 결함, 네트워크 오류, 또는 시장의 급격한 변동으로 인해 발생하는 어떠한 금전적 손실에 대해서도 본 소프트웨어의 개발자는 일절 책임을 지지 않습니다.

사용자는 본 프로그램을 사용하기 전에 반드시 모의투자 환경에서 충분한 테스트를 거쳐야 합니다.

[🇺🇸 English] (Bonus - Recommended for GitHub)

HappyFuture - C# Overseas Futures Automated Trading System
This is an automated overseas futures trading bot developed in C# (.NET Framework, WinForms). It utilizes the Kiwoom Securities API to provide real-time data reception, strategy backtesting, and automated order execution.

📌 Core Features

Kiwoom API Integration: Wraps the AxKFOpenAPI (COM) component in C# to handle real-time data and order execution for overseas futures.

State Machine Driven: Utilizes a robust State Machine (FutureState, ListUpFuturePhase, WatchingFuturePhase) for stable and predictable bot operation.

Strategy Backtesting: Employs FutureBackTestRecoder to validate trading strategies against historical data.

Live Automated Trading: Automatically enters and exits positions based on defined logic within FutureFundManagement.

Data Persistence: Uses SQLite (FuturePriceDB, FutureTradeTecodeDB) to permanently store candlestick data and trade history.

Real-time Notifications: Integrates with a Telegram bot to send live alerts for executed orders, errors, and profit/loss summaries.

🛠️ Technology Stack

Language: C# (.NET Framework, WinForms)

API: Kiwoom Overseas Futures API (COM)

Database: SQLite

Notifications: Telegram API

⚠️ Disclaimer

This software is an automated trading system that operates with real money. The user assumes all responsibility for any and all trades.

The developer of this software is not liable for any financial losses incurred due to software bugs, logical flaws, network errors, or sudden market volatility.

Users MUST thoroughly test this program in a simulated (demo) environment before deploying it with real funds.

[🇨🇳 繁體中文]

HappyFuture - C# 海外期貨自動交易系統
這是一個使用 C# (.NET Framework, WinForms) 開發的海外期貨自動交易機器人。它利用韓國 Kiwoom 證券 API，提供即時數據接收、策略回測和自動訂單執行功能。

📌 核心功能

Kiwoom API 整合: 使用 C# 封裝 AxKFOpenAPI (COM) 元件，用於處理海外期貨的即時數據和訂單執行。

狀態機管理: 透過 FutureState (ListUpFuturePhase, WatchingFuturePhase 等) 對機器人進行穩定的狀態管理，提高系統可靠性。

策略回測: 使用 FutureBackTestRecoder 功能，基於歷史數據驗證交易策略的有效性。

即時自動交易: 透過 FutureFundManagement 及其策略模組，根據設定的邏輯自動進場與出場。

數據持久化: 使用 SQLite 數據庫 (FuturePriceDB, FutureTradeTecodeDB) 永久儲存 K 線數據與所有交易紀錄。

即時通知: 整合 Telegram 機器人，即時發送訂單成交、錯誤訊息、當日損益等重要事件的通知。

🛠️ 主要技術

語言: C# (.NET Framework / WinForms)

API: 韓國 Kiwoom 證券海外期貨 API (COM)

數據庫: SQLite

通知: Telegram API

⚠️ 免責聲明

本軟體是涉及真實資金的自動交易系統。所有交易的最終責任均由使用者承擔。

對於因軟體錯誤、邏輯缺陷、網路問題或市場劇烈波動而導致的任何財務損失，本軟體開發者概不負責。

使用者在投入真實資金之前，必須在模擬交易環境中進行充分的測試。

[🇯🇵 日本語]

HappyFuture - C# 海外先物自動売買トレーディングシステム
C# (.NET Framework, WinForms) で開発された海外先物自動売買ボットです。キウム証券APIを活用し、リアルタイムデータ受信、戦略バックテスト、自動注文実行機能を提供します。

📌 主な機能

Kiwoom API連携: AxKFOpenAPI (COM) コンポーネントをC#でラッピングし、海外先物のリアルタイム時価および注文機能を実装します。

状態マシンによる運用: FutureState (ListUpFuturePhase, WatchingFuturePhase など) を通じてボットの動作状態を明確に管理し、安定性を向上させました。

戦略バックテスト: FutureBackTestRecoder を利用し、過去データに基づいて売買戦略の有効性を検証します。

リアルタイム自動売買: FutureFundManagement および下位戦略モジュールを通じて、設定されたロジックに従い自動でポジションのエントリーおよび決済を行います。

データの永続化: SQLiteデータベース (FuturePriceDB, FutureTradeTecodeDB) を使用し、分足データとすべての売買履歴を永続的に保存・管理します。

リアルタイム通知: Telegramボットと連携し、注文約定、エラー発生、日次損益などの主要なイベントをリアルタイムでユーザーに通知します。

🛠️ 主な技術スタック

言語: C# (.NET Framework / WinForms)

API: キウム証券 海外先物API (COM)

データベース: SQLite

通知: Telegram API

⚠️ 免責事項

本ソフトウェアは、実際の資金が運用される自動売買システムです。すべての売買に関する最終的な責任はユーザーにあります。

ソフトウェアのバグ、ロジックの欠陥、ネットワークエラー、または市場の急激な変動によって発生するいかなる金銭的損失についても、本ソフトウェアの開発者は一切の責任を負いません。

ユーザーは、本プログラムを使用する前に、必ずデモ取引環境で十分なテストを行う必要があります。
