window.initNumericFormat = function (container) {
    if (!container) return;
    const input = container.querySelector('input');
    if (!input || input._numericFormatInitialized) return;
    input._numericFormatInitialized = true;

    input.addEventListener('input', function () {
        const selStart = input.selectionStart;
        const oldVal = input.value;

        // Count meaningful characters (digits and decimal point) before cursor
        let meaningfulBeforeCursor = 0;
        for (let i = 0; i < selStart; i++) {
            if (/\d/.test(oldVal[i]) || oldVal[i] === '.') meaningfulBeforeCursor++;
        }

        const newVal = formatNumeric(oldVal);
        if (newVal === oldVal) return;
        input.value = newVal;

        // Restore cursor position by counting same number of meaningful chars in new value
        let count = 0;
        let newPos = newVal.length;
        if (meaningfulBeforeCursor === 0) {
            newPos = 0;
        } else {
            for (let i = 0; i < newVal.length; i++) {
                if (/\d/.test(newVal[i]) || newVal[i] === '.') {
                    count++;
                    if (count === meaningfulBeforeCursor) {
                        newPos = i + 1;
                        break;
                    }
                }
            }
        }
        input.setSelectionRange(newPos, newPos);
    });
};

window.getNumericInputValue = function (container) {
    if (!container) return '';
    const input = container.querySelector('input');
    return input ? input.value : '';
};

function formatNumeric(value) {
    let intPart = '';
    let decPart = '';
    let hasDecimal = false;

    for (const c of value) {
        if (/\d/.test(c)) {
            if (hasDecimal) decPart += c;
            else intPart += c;
        } else if (c === '.' && !hasDecimal) {
            hasDecimal = true;
        }
    }

    if (!intPart && !hasDecimal) return '';

    const formatted = intPart ? Number(intPart).toLocaleString('en-US') : '0';

    return hasDecimal ? formatted + '.' + decPart : formatted;
}
