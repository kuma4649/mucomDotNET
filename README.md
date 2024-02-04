# mucom.NET  
  mucom88の.NET版です。  
  
[概要]  
  mucom88を.NET版に移植したものです。  
  OPNAx2,OPNBx2,OPMx1の同時使用が可能です。  
  (ぼうきちさんのWの機能を盛り込んだ形です。Thanks!>ぼうきちさん)  
  古代さんのご厚意でAMD98の機能も盛り込んでおります。  
  公式ページ  
- [OPEN MUCOM PROJECT (株式会社エインシャント様)](https://www.ancient.co.jp/~mucom88/)  
- [OPEN MUCOM88 Wiki (ONION software/おにたま様)](https://github.com/onitama/mucom88/wiki)  
  
[機能、特徴]  
 ・１パートを最大10ページに分けて記述できます。  
 ・１ページ毎に64Kbyteフルに使用したmubが作成できます。未確認。  
 ・一部mucom88ではオミットされた機能などが使えちゃいます。  
 このため、それらを使用したデータをmucom88で演奏するとおかしな事態になってしまいます。  
 (今のところ対策は何も行われていません。)  
  
[必要な環境]  
 ・Windows7以降のOSがインストールされたPC  
 ・テキストエディタ  
 ・気合と根性  
  
[著作権・免責]  
mucom.NETはGPLv3ライセンスとします。  
著作権は作者が保有しています。  
このソフトは無保証であり、このソフトを使用した事による  
いかなる損害も作者は一切の責任を負いません。  
  
以下のソフトウェアのソースコードをC#向けに改変し使用しています。  
これらのソースは各著作者が著作権を持ちます。  
ライセンスに関しては、各ドキュメントを参照してください。  
  
  ・EncAdpcmA.cs  参考元：https://wiki.neogeodev.org/index.php?title=ADPCM_codecs  
  
  
以下のソフトウェアのソースコードをC#向けに改変し使用しています。  
又はコード/dllを使用させていただいております。  
これらのソース/バイナリは各著作者が著作権を持ちます。  
ライセンスに関しては、各ドキュメントを参照してください。  
  
 ・mucom88/mucom88win   -> CC BY-NC-SA 4.0 -> コード改変  
 ・AMD98                -> ?               -> コード改変  
 ・MDSound              -> GPLv3           -> dll動的リンクで使用  
 ・musicDriverInterface -> MIT             -> dll動的リンクで使用  
 ・RealChipCtlWrap      -> MIT             -> dll動的リンクで使用  
 ・NAudio               -> MS-PL           -> dll動的リンクで使用  
 ・SCCI                 -> ?               -> dll動的リンクで使用  
 ・c86ctl               -> BSD 3-Clause    -> dll動的リンクで使用  
  
  
[SpecialThanks]  
 本ツールは以下の方々にお世話になっております。また以下のソフトウェア、ウェブページを参考、使用しています。  
 本当にありがとうございます。  
 ・古代 さん(おぉ～!!)  
 ・くろま さん  
 ・mucom さん  
 ・ぼうきち さん  
 ・TAN-Y さん  
 ・ゆきにゃん さん  

 ・mucom88/mucom88win  
 ・Music LALF  
 ・MXDRV  
 ・MNDRV  
 ・Visual Studio Community 2019  
 ・さくらエディター  
  
 ・[mucomさんとこのwiki](https://github.com/MUCOM88/mucom88/wiki)  
 ・[ぼうきちさんとこのwiki](https://github.com/BouKiCHi/mucom88/wiki)  
