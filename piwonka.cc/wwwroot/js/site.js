// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Code-Highlighting mit Prism.js aktualisieren, wenn die Seite geladen wird
document.addEventListener('DOMContentLoaded', function () {
    // Falls Prism schon geladen wurde
    if (typeof Prism !== 'undefined') {
        Prism.highlightAll();
    }
});

// Für TinyMCE-Integration: Wenn TinyMCE einen Code-Block einfügt, formatieren wir ihn
function refreshPrismCodeBlocks() {
    if (typeof Prism !== 'undefined') {
        // Alle Code-Blöcke finden, die noch nicht hervorgehoben wurden
        const codeBlocks = document.querySelectorAll('pre:not(.line-numbers) > code:not([class*="language-"])');

        // Klassen hinzufügen und Prism anwenden
        codeBlocks.forEach(function (block) {
            const parentPre = block.parentNode;
            if (parentPre && parentPre.tagName === 'PRE') {
                parentPre.classList.add('line-numbers');

                // Versuchen, die Language zu erkennen
                let language = 'markup'; // Standard ist HTML/Markup
                const code = block.textContent;

                if (code.includes('class=') || code.includes('<div') || code.includes('<span')) {
                    language = 'markup';
                } else if (code.includes('function') || code.includes('var ') || code.includes('const ') || code.includes('let ')) {
                    language = 'javascript';
                } else if (code.includes('namespace') || code.includes('using System') || code.includes('public class')) {
                    language = 'csharp';
                }

                block.classList.add(`language-${language}`);
            }
        });

        // Prism nochmal anwenden
        Prism.highlightAll();
    }
}

// Wenn die Seite geladen ist
window.addEventListener('load', refreshPrismCodeBlocks);