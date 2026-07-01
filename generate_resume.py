import subprocess
import os

resume_html = os.path.abspath("resume.html")
pdf_path = os.path.abspath("小番茄_AI应用工程师_简历.pdf")

edge_paths = [
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    r"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
]

edge = None
for p in edge_paths:
    if os.path.exists(p):
        edge = p
        break

if not edge:
    raise FileNotFoundError("Edge browser not found")

cmd = [
    edge,
    "--headless",
    f"--print-to-pdf={pdf_path}",
    "--no-pdf-header-footer",
    f"file:///{resume_html.replace(os.sep, '/')}",
]
result = subprocess.run(cmd, capture_output=True, text=True)
print(f"PDF generated: {pdf_path}")
print(f"Size: {os.path.getsize(pdf_path)} bytes")
