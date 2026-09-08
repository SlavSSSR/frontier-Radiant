#!/usr/bin/env python3

import subprocess
from typing import Iterable

def main() -> int:
    any_failed = False
    for file_name in get_text_files():
        file_name = file_name.strip('"')

        # --- ПОЛНОЦЕННЫЙ ДЕКОДЕР КИРИЛЛИЦЫ ДЛЯ LINUX ---
        # Если Git выдал путь с экранированными байтами (например, \\320\\241),
        # мы принудительно превращаем их обратно в нормальные русские буквы.
        if "\\" in file_name:
            try:
                # Декодируем экранированные слэши Git в честный UTF-8 путь
                file_name = file_name.encode('utf-8').decode('unicode_escape').encode('latin1').decode('utf-8')
            except Exception:
                pass # Если что-то пошло не так, оставляем как есть
        # -----------------------------------------------

        # Теперь Python легко откроет файл, даже если в пути есть русские буквы!
        try:
            if is_file_crlf(file_name):
                print(f"::error file={file_name},title=File contains CRLF line endings::The file '{file_name}' was committed with CRLF new lines. Please make sure your git client is configured correctly and you are not uploading files directly to GitHub via the web interface.")
                any_failed = True
        except FileNotFoundError:
            # Страховка на случай, если файл физически удалён из ветки
            continue

    return 1 if any_failed else 0



def get_text_files() -> Iterable[str]:
    # https://stackoverflow.com/a/24350112/4678631
    process = subprocess.run(
        ["git", "grep", "--cached", "-Il", ""],
        check=True,
        encoding="utf-8",
        stdout=subprocess.PIPE)

    for x in process.stdout.splitlines():
        yield x.strip()

def is_file_crlf(path: str) -> bool:
    # https://stackoverflow.com/a/29697732/4678631
    with open(path, "rb") as f:
        for line in f:
            if line.endswith(b"\r\n"):
                return True

    return False

exit(main())
