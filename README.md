# Csharp-Multiloader

Old WinForms project: particle background, Guna UI, and a small auto-updater on startup. Originally written by **redskyservice**, later rebranded to **Oniware**. We’re not developing this anymore; it’s here for anyone who wants to poke around or fork it.

Stack is nothing fancy: **.NET Framework 4.7.2**, output assembly name `oniware_multiloader`. Guna.UI2 is pulled in via NuGet and embedded so you don’t ship a loose `Guna.UI2.dll` next to the exe (see `Program.cs` and the embedded resource). On launch, `Form2` handles updates, then `Form1` is the main UI.

Open `redskyservice_multi_loaderv2.sln`, restore packages (Fody / PropertyChanged / Guna), build Release or Debug. Binaries land in `bin\Release` or `bin\Debug` like any other WinForms app.

---

### Auto-updater (what the client actually does)

All of this update logic is in `Form2.cs`.

`API_URL` at the top is your server base (e.g. `http://1.2.3.4:5000`). On load it hits:

`GET /check_oni_update`

It expects JSON with at least a `version` field. That string is parsed with `System.Version` and compared to `CURRENT_VERSION` in the same file. If the server’s version is **higher**, it shows the update dialog. If the user says yes, it needs a `download_url` in that JSON (full URL, not a path) and it will `GET` that and swap the exe via a temp batch file. So: no `download_url`, no update install, even if you thought you advertised an update.

The `update_available` field in your API response doesn’t drive anything here; the WinForms code never checks it. Only the numeric/string version comparison matters.

When you release a new build, bump `CURRENT_VERSION` in `Form2` to match what you’re shipping, otherwise people on old builds will keep getting prompted forever.

`AssemblyInfo.cs` still has leftover template names in places. For “what version am I?” trust `Form2`, not the assembly metadata.

---

### Flask side (example)

If your backend is Flask, this is enough to stay compatible: one route that returns version + download link, one that sends the file.

Point `UPDATE_FILE_PATH2` at wherever the new `oniware_multiloader_updated.exe` actually lives on disk (the path below is just a placeholder).

```python
CURRENT_VERSION2 = "1.4"
UPDATE_FILE_PATH2 = r"C:\path\to\oniware_multiloader_updated\oniware_multiloader_updated.exe"
```

```python
from flask import Flask, jsonify, request, send_file
import os

app = Flask(__name__)

@app.route('/check_oni_update', methods=['GET'])
def check_oni_update():
    return jsonify({
        "update_available": True,
        "version": CURRENT_VERSION2,
        "download_url": f"http://{request.host}/download_oni_update"
    })

@app.route('/download_oni_update', methods=['GET'])
def download_oni_update():
    if os.path.exists(UPDATE_FILE_PATH2):
        return send_file(UPDATE_FILE_PATH2, as_attachment=True)
    else:
        return "Update file not found", 404
```

`request.host` helps generate a download URL that matches the API host the client used.
If you run behind Nginx (or another reverse proxy), this value can be wrong unless forwarded headers are configured correctly.
If needed, set your public download URL manually.

Use HTTPS in production. If the update file is sensitive, protect the download endpoint.

---

### Small Note:

No warranty, no support, use at your own risk. The rest of the app can hit the network, ask for admin, or run batch stuff depending on what you click. Read the code before you run it on anything important.

**Credits:** redskyservice.
