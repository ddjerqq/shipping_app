import xml.etree.ElementTree as ET
from googletrans import Translator
import sys
import asyncio as aio

async def translate_resx(input_file, output_file, target_lang):
    translator = Translator()

    tree = ET.parse(input_file)
    root = tree.getroot()

    # Namespace handling for .resx files
    ns = {'resx': 'http://www.w3.org/2001/XMLSchema-instance'}

    for data in root.findall(".//data"):
        value_element = data.find("value")
        if value_element is not None and value_element.text:
            original_text = value_element.text.strip()
            translated_text = (await translator.translate(original_text, dest=target_lang)).text
            value_element.text = translated_text
            print(f"Translated: '{original_text}' -> '{translated_text}'")

    tree.write(output_file, encoding="utf-8", xml_declaration=True)
    print(f"Translated .resx file saved as {output_file}")

async def main():
    if len(sys.argv) != 4:
        print("Usage: python translate_resx.py <input.resx> <output.resx> <target_language>")
        sys.exit(1)

    input_resx = sys.argv[1]
    output_resx = sys.argv[2]
    target_language = sys.argv[3]

    await translate_resx(input_resx, output_resx, target_language)


if __name__ == "__main__":
    aio.run(main())