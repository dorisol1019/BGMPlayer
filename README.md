
# BGM鳴ら～すV3

  Copyright (c) 2017-2019 dorisol1019

![BGMPlayer](https://github.com/dorisol1019/BGMPlayer/blob/images/BGMPlayer.jpg)

## 概要

dorisol1019が欲しい機能を詰め込んだフリーの音楽プレーヤーソフトです

## 再生可能なフォーマット

* midiファイル
* waveファイル
* mp3ファイル
* oggファイル

## 機能

* 再生・停止
* 一時停止・一時停止解除
* midiファイルは#CC111ループ対応
* リストの順番通りの再生/シャッフル再生

## 動作要件

* Windows OSであること
* .NET Core 3.1の動作要件を満たしていること  
  https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md

## インストール

ダウンロードページはこちらになります  
https://github.com/dorisol1019/BGMPlayer/releases

.NET Core 3.1(x86)をインストールしている人は「BGMPlayer.zip」をダウンロードして、好きなところに解凍してください。

.NET Core 3.1(x86)をインストールしていない人は「BGMPlayer_runtime_bundle.zip」をダウンロードするか、  
もしくは以下のページから .NET Core Runtime/.NET Core Desktop Runtime（両方ともx86）をインストールしたうえで「BGMPlayer.zip」をダウンロードしてください。

.NET Core ダウンロードページ  
https://dotnet.microsoft.com/download/dotnet-core/3.1/runtime

## 注意事項

* 大きいサイズ(数十MB)のファイルを読み込むと、ソフトが異常停止するおそれがあります
　
* 「#CC111が最後の発音のあとにある」midiファイルはループ再生しませんのでお気をつけください

## 使用方法

1. Playlistフォルダに楽曲を入れます  
   Playlistフォルダがない場合は、本ソフトを起動していただくと作成されます
2. 本ソフトを起動します
3. 気分でループや音量の設定をします
4. 再生ボタンを押して楽曲を楽しみます
5. 止めたければ停止ボタンを押します
6. 「ファイル(F)」→「フォルダを開く(O)」で楽曲を読み込むフォルダを変更することができます

## 削除、アンインストール

このフォルダごとゴミ箱にぶち込んでください

## 著作権

本ソフトはフリーですが、著作権は放棄していません  
著作権はdorisol1019に帰属します  
また、使用したライブラリの著作権はライブラリ作者に帰属します  

## 連絡先

バグ報告、苦情、要望等は以下までお願いします

twitter.com @dorisol1019
