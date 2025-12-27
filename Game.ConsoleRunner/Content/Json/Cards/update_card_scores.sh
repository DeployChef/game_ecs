#!/bin/bash

# Скрипт для обновления всех JSON файлов карт с BaseScore
# Правила Balatro:
# - Ace = 11
# - King, Queen, Jack = 10
# - 2-10 = номинал

CARDS_DIR="$(dirname "$0")"

# Функция для обновления одного файла
update_card() {
    local file="$1"
    local rank="$2"
    local score="$3"
    
    # Используем Python для обновления JSON (более надежно чем sed)
    python3 <<EOF
import json
import sys

file_path = "$file"
rank = "$rank"
score = $score

try:
    with open(file_path, 'r') as f:
        data = json.load(f)
    
    data['BaseScore'] = score
    
    with open(file_path, 'w') as f:
        json.dump(data, f, indent=2)
        f.write('\n')
    
    print(f"Updated {file_path}: BaseScore = {score}")
except Exception as e:
    print(f"Error updating {file_path}: {e}", file=sys.stderr)
    sys.exit(1)
EOF
}

# Обновляем все карты
for suit in Spades Hearts Diamonds Clubs; do
    # Ace = 11
    update_card "${CARDS_DIR}/Ace_${suit}.json" "Ace" 11
    
    # King, Queen, Jack = 10
    update_card "${CARDS_DIR}/King_${suit}.json" "King" 10
    update_card "${CARDS_DIR}/Queen_${suit}.json" "Queen" 10
    update_card "${CARDS_DIR}/Jack_${suit}.json" "Jack" 10
    
    # Ten = 10
    update_card "${CARDS_DIR}/Ten_${suit}.json" "Ten" 10
    
    # 2-9 = номинал
    for rank in Two Three Four Five Six Seven Eight Nine; do
        case $rank in
            Two) score=2 ;;
            Three) score=3 ;;
            Four) score=4 ;;
            Five) score=5 ;;
            Six) score=6 ;;
            Seven) score=7 ;;
            Eight) score=8 ;;
            Nine) score=9 ;;
        esac
        update_card "${CARDS_DIR}/${rank}_${suit}.json" "$rank" "$score"
    done
done

echo "All card files updated with BaseScore!"

