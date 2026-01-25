window.quillInterop = {
    create: function (editorId) {
        const q = new Quill("#" + editorId, { theme: "snow" });
        return q;
    },
    setHtml: function (editorId, html) {
        const el = document.querySelector("#" + editorId + " .ql-editor");
        if (el) el.innerHTML = html || "";
    },
    getHtml: function (editorId) {
        const el = document.querySelector("#" + editorId + " .ql-editor");
        return el ? el.innerHTML : "";
    }
};
