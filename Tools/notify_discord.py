#!/usr/bin/env python3

import requests
import os
import json
import re
from typing import Union

PR_NOTIFY_WEBHOOK = os.getenv("PR_NOTIFY_WEBHOOK", "")
PR_BODY = os.getenv("PR_BODY", "")
PR_AUTHOR = os.getenv("PR_AUTHOR", "")
PR_URL = os.getenv("PR_URL", "")

author_regex = r':cl:([ \S]*)'
content_regex = r'-\s+(add|remove|fix|tweak): +(.*)'

def main():
    content = format_body(PR_BODY)
    content = format_authors(content, PR_URL, PR_AUTHOR)
    if (content == False or content.isspace() == True):
        print("В тексте не найдено :cl: тега и элементов add/remove... и т.д.")
        return
    
    print(f"Итоговая версия оповещения: {content}")

    send_discord(pr_notify_webhook=PR_NOTIFY_WEBHOOK, content=content)

def send_discord(content: str, pr_notify_webhook: str):
    data = {
        "content": content
    }
    headers = {
        "Content-Type": "application/json"
    }
    response = requests.post(pr_notify_webhook, data=json.dumps(data), headers=headers)
    print(response.text)
    response.raise_for_status()
    print("Оповещение прошло успешно!")

def format_body(body: str) -> str:
    result = ""

    debug_content_matches_count = 0

    content_matches = re.finditer(content_regex, body, re.IGNORECASE)
    for match in content_matches:
        cl_type = match.group(1)
        message = match.group(2).capitalize().strip()

        if cl_type == "add":
            result += '🆕'
        elif cl_type == "tweak":
            result += '🛠️'
        elif cl_type == "fix":
            result += '🐛'
        elif cl_type == "remove":
            result += '❌'
        elif cl_type == "debug":
            result += "pup_debug_string"
        
        print("------")
        print(f"CL_TYPE: {cl_type}")
        print(f"MESSAGE: {message}")
        print("------")

        result += f" {message}\n"
        debug_content_matches_count += 1

    print(f"CONTENT_MATCHES: {debug_content_matches_count}")
    if debug_content_matches_count == 0:
        return ""

    return result

def format_authors(body: str, pr_url: Union[str, None], pr_base_author: str) -> str:
    if (body == "" or body.isspace()):
        return ""

    result = body
    authors = [pr_base_author]

    author_match = re.search(author_regex, body, re.IGNORECASE) 
    if author_match:
        authors += author_match.group(1).split()

    print(f"AUTHOR_MATCHES: {len(authors)}")

    result += f"\nАвторы: "
    for author in authors:
        result += author

    if pr_url != None:
        result += f" [PR]({pr_url})"

    return result

if __name__ == '__main__':
    main()
