# Chrome Profil Yedekleme

Google Chrome profillerinizi yedekleyip format sonrası geri yükleyen Windows uygulaması.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6)

## Özellikler

- Tüm Chrome profillerini listeleme (e-posta, yer imi, şifre, eklenti, boyut)
- **Seçili profil yedekleme** — hangi profillerin yedekleneceğini seçin
- **Yedek dışı bırakma** — şifre, geçmiş, çerez, eklenti, önbellek hariç tutulabilir
- **Seçili profil geri yükleme** — tek veya birden fazla profil
- Google hesapları + site giriş e-postaları
- Yer imi, şifre, geçmiş, eklenti detayları
- **CSV dışa aktarma** (yer imi, şifre, e-posta, geçmiş)
- Yedek karşılaştırma (iki yedeği yan yana)
- Zamanlanmış otomatik yedek (Windows Görev Zamanlayıcı)
- Yedek klasörünü seçme (USB, OneDrive, D: vb.)

## Gereksinimler

- Windows 10/11 (64-bit)
- [.NET 9 SDK](https://dotnet.microsoft.com/download) (derlemek için)

## Kurulum (kullanıcı)

1. [Releases](https://github.com/SoulzHem/ChromeProfilYedekleme/releases) sayfasından `ChromeProfilYedek.exe` indirin  
   *(veya kendiniz derleyin — aşağıya bakın)*
2. EXE'yi istediğiniz klasöre koyun
3. Çift tıklayarak açın

Not: Son sürümlerde uygulama tek-dosya (self-contained) olarak yayınlanmaktadır — çalıştırmak için sistemde .NET SDK/Runtime kurulmasına gerek yoktur. Tek dosya boyutu platforma göre ~110-120 MB civarındadır.

## Derleme (geliştirici)

Projeyi yerel olarak derlemek veya kendi EXE'nizi üretmek için `Derle.bat` kullanabilirsiniz. Bu script, `dotnet publish` ile tek-dosya (self-contained) bir EXE üretir ve köke kopyalar.

Kısa kullanım (PowerShell):

```powershell
cd ChromeProfilApp
.
\..\Derle.bat
```

Veya manuel:

```powershell
cd ChromeProfilApp
dotnet publish -c Release -o ..\publish -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true
copy ..\publish\ChromeProfilYedek.exe ..\ChromeProfilYedek.exe
```

Çıktı: `ChromeProfilYedek.exe` (self-contained, native sqlite dahil edecek biçimde paketlenir). Tek dosya paket boyutu ~110-120 MB olabilir.

## Kullanım

### Yedekleme
1. Profilleri **Yedekle** sütunundan seçin
2. **Yedekle** → **Yedek Ayarları**'ndan istemediğiniz kısımları kapatın
3. **Yedeklemeyi Başlat**
4. `Yedekler` klasörünü USB/buluta kopyalayın

### Geri yükleme
1. Chrome kurun
2. EXE + `Yedekler` klasörünü bilgisayara kopyalayın
3. **Geri Yükle** → hangi profillerin geleceğini seçin

### Zamanlanmış yedek
**Zamanlı Yedek** butonu → görev oluşturur. Chrome açıksa o çalışmada yedek atlanır.

Komut satırı (otomatik):
```bat
ChromeProfilYedek.exe --otomatik-yedek
```

## Proje yapısı

```
chromeprofil/
├── ChromeProfilApp/     # Kaynak kod (.NET 9 WinForms)
├── Derle.bat            # EXE derleme
├── .gitignore
└── README.md
```

## Önemli notlar

- Yedekleme/geri yükleme sırasında **Chrome kapalı** olmalı
- Format sonrası bazı şifreler Windows hesabına bağlı olduğu için çalışmayabilir
- Google hesabıyla giriş yaptığınız profiller genelde tekrar oturum açar

Tek-dosya (single-file) paketlerde native SQLite yükleyicisinin (e_sqlite3) doğru çalışması için proje `Microsoft.Data.Sqlite` + `SQLitePCLRaw` bundle kullanmaktadır. Eğer çalıştırma sırasında "DLL not found" benzeri bir hata alırsanız:

1. Uygulamayı aynı klasörde çalıştırmayı deneyin (bazı antivirüs/izinler extract sürecini engelleyebilir).
2. Eğer sorun devam ederse, `startup-error.log` dosyasını uygulama klasöründe kontrol edip hata detayını bizimle paylaşın.
3. Geliştirici olarak derliyorsanız `Derle.bat` veya `dotnet publish` parametrelerini kullanın; proje `Program.Main` içinde `SQLitePCL.Batteries_V2.Init()` çağrısı yapılmaktadır.

## Windows Defender uyarısı

Bu uygulama **açık kaynaklı** bir yedekleme aracıdır; imzasız tek EXE dosyaları ve Chrome profil/şifre dosyalarına erişim Defender tarafından yanlışlıkla “zararlı” sayılabilir.

**v1.0.1’de yapılan iyileştirmeler:**
- Sıkıştırılmış self-extract EXE kapatıldı (en sık tetikleyen neden)
- Ürün metadata’sı ve uygulama manifest’i eklendi
- Şüpheli COM şifre çözme kodu kaldırıldı

**Hâlâ uyarı alırsanız:**
1. [GitHub Releases](https://github.com/SoulzHem/ChromeProfilYedekleme/releases) üzerinden indirdiğinizden emin olun
2. Windows Güvenliği → Virüs ve tehdit koruması → **İstisnalar** → EXE klasörünü ekleyin
3. [Microsoft yanlış pozitif bildirimi](https://www.microsoft.com/en-us/wdsi/filesubmission) gönderin (dosya: `ChromeProfilYedek.exe`, geliştirici: SoulzHem)



## Lisans

MIT
