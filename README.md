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

## Derleme (geliştirici)

```bat
Derle.bat
```

veya:

```powershell
cd ChromeProfilApp
dotnet publish -c Release -o ..\publish
copy ..\publish\ChromeProfilYedek.exe ..\ChromeProfilYedek.exe
```

Çıktı: `ChromeProfilYedek.exe` (~48 MB, tek dosya, .NET kurulumu gerekmez)

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

Kalıcı çözüm için kod imzalama sertifikası gerekir (ücretli).

## Lisans

MIT
