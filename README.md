# Unity PSG Player

Unity上でレトロゲーム機などのPSG（Programmable Sound Generator）音源（いわゆる8ビットサウンド・ピコピコサウンド）を鳴らすライブラリです。  
演奏データはMML（Music Macro Language）のテキストで、楽譜を文字で記述するため、手軽に音楽データを作成できます。  
ファミコンの音源に似た（DPCMを除く）表現ができるように設計しています。  


## できること

* 音色は、矩形波4種類、三角波、ノイズ2種類が鳴らせます。三角波は4bit波形です。
* 演奏表現として、スイープ、LFO（ビブラート）、音量エンベロープが使えます。

> ファミコン音源の完全再現は目標にしていません。（めんどくさいので。）  
> あくまでも、別途サウンドクリップなどを用意しないで、Unityだけで音を鳴らす目的で制作しています。  

詳細は[マニュアル](./Unity%20PSG%20Player%20-%20manual.md)・[MMLリファレンス](./Unity%20PSG%20Player%20-%20MML%20reference.md)を参照してください。


## 使用用途

* レトロ調ゲームのBGMや効果音に。
* ちょっとした効果音を一々作ったり探したりするのが面倒なとき。

## 動作環境

* Unity 2022.3(LTS)以上

> 上記以前のバージョンでも動作する気はしますが未確認。


## 対応プラットホーム

* 動作確認済み
  * Windows
  * Android
* 非対応（現状）
  * Web GL
* 未検証
  * 上記以外

> Web GLは、ブラウザーが動的ストリーミングをサポートしていないので、現状は非対応。  
> その他プラットホームは、単に自分では確認できる環境がないので、報告してもらえると助かります。


## クイックガイド

### 基本的な使い方

1. PSG Playerプレハブをヒエラルキーに置きます。  
![fig01](./img/fig01.png)

2. 操作するスクリプトでPSGPlayerクラス変数を用意し、設置したPSG Playerオブジェクトをアタッチします。  
![fig02](./img/fig02.png)

3. PSGPlayerの[mmlString](#mmlstring)変数に記述したMMLが、[Play()](#play)で再生されます。  
MMLについては「[MMLリファレンス](Unity%20PSG%20Player%20-%20MML%20reference.md)」を参照してください。  
![fig03](./img/fig03.png)


### マルチチャンネルの使い方

1. 必要なチャンネル数に合わせてPSG Playerプレハブを置きます。  
![fig04](./img/fig04.png)

2. 適当なゲームオブジェクトにMMLSplitterスクリプトをアタッチします。  
3. インスペクターからMMLSplitterの[psgPlayers](#psgplayers)に、設置したPSG Playerを割り当てます。  
![fig05](./img/fig05.png)

4. MMLSplitterの[multiChMMLString](#multichmmlstring)変数にMMLを入れて、[SplitMML()](#splitmml)で各チャンネルにMMLを分配し、[PlayAllChannels()](#playallchannels)で再生されます。  
![fig06](./img/fig06.png)


## アップデート（するかも）予定

* シーケンスデータのJSON化

> MMLをデコードするコストが減るので、CPU負荷の軽減ができるかも？

* 非ストリーム再生

> 効果音などは、プリレンダリングして波形データを用意しておくことで、レスポンスが良くなりそう。

* Web GLに対応

> Webでも鳴らせる方法があるみたいだけど、統合するかは未定。

* サンプル曲と効果音をオリジナルに変更

> 耳コピは得意なんですが作曲は苦手で。。。  
> どなたか作ってくれる方がいらっしゃいましたらご協力お願いしますm(_ _)m  


## ライセンス

* MITライセンス
* This library is under the MIT License.

