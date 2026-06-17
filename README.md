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

1. [Releases](https://github.com/KULLANICI/chromeprofil/releases) sayfasından `ChromeProfilYedek.exe` indirin  
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

## Lisans

MIT
