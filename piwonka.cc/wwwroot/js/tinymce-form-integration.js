
(function () {
    'use strict';

    // Funktion zum Aktualisieren aller TinyMCE-Editoren vor Form-Submit
    function updateAllTinyMCEEditors() {
        if (typeof tinymce !== 'undefined') {
            tinymce.editors.forEach(function (editor) {
                if (editor && editor.save) {
                    editor.save();
                    console.log('Editor aktualisiert:', editor.id);
                }
            });
        }
    }

    // Event-Listener für alle Formulare hinzufügen
    document.addEventListener('DOMContentLoaded', function () {
        // Alle Formulare finden, die TinyMCE-Editoren enthalten könnten
        const forms = document.querySelectorAll('form');

        forms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                updateAllTinyMCEEditors();
            });
        });

        // Zusätzlich: Bei allen Submit-Buttons
        const submitButtons = document.querySelectorAll('button[type="submit"], input[type="submit"]');

        submitButtons.forEach(function (button) {
            button.addEventListener('click', function (e) {
                updateAllTinyMCEEditors();
            });
        });
    });
})();