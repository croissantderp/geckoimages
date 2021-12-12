document.addEventListener('DOMContentLoaded', function () { Initialrender(); initiate(); });

var params;
var query = "";
var currentQuery = "";

function initiate() {
    // Icon Click Focus
    $('div.icon').click(function () {
        $('input#search').focus();
    });

    //Listen for the event
    $("input#search").on("keyup", function (e) {
        // Set Search String
        query = $(this).val();
        params.set("q", $(this).val());
    });

    document.getElementById('searchBar').onkeypress = function (e) {
        if (!e) e = window.event;
        var keyCode = e.code || e.key;
        if (keyCode == 'Enter') {

            if (query != "") {
                document.location = "browse.html?" + params.toString();
            }
            else {
                document.location = "browse.html";
            }
        }
    }

    setInterval(queueSearch, 500);
}

function queueSearch() {
    if (query != currentQuery) {
        render(query);
        currentQuery = query;
    }
}

var geckos = [];
var geckoList = {};
var geckoDiv;

function render(query = "") {
    for (let i = 0; i < geckos.length; i++) {
        if (geckos[i]["name"].toLowerCase().includes(query.toString().toLowerCase().replace(" ", "_"))) {
            geckoList[geckos[i]["name"]].style.display = "inline-grid";
        }
        else {
            geckoList[geckos[i]["name"]].style.display = "none";
        }
    }
}

function Initialrender() {
    params = new URLSearchParams(window.location.href.split("?")[1]);

    $.get("https://geckoimages.ddns.net/db.json", function (data) {

        document.getElementById("APICheck").innerHTML = '';

        geckos = JSON.parse(data);

        let az = document.getElementById("az");

        let num = document.getElementById("num");

        switch (params.get('sort')) {
            case "09":
                geckos.sort(function (a, b) {
                    if (a["name"] > b["name"]) return 1;
                    if (a["name"] < b["name"]) return -1;
                    else return 0;
                });
                break;

            case "90":
                numReversed = true;
                num.textContent = "9-0"

                geckos.sort(function (a, b) {
                    if (a["name"] > b["name"]) return 1;
                    if (a["name"] < b["name"]) return -1;
                    else return 0;
                });
                geckos.reverse();
                break;

            case "az":
                azSelected = true;
                num.className = "unselected";
                az.className = "selected";

                geckos.sort(function (a, b) {
                    c = a["name"].split("_");
                    d = b["name"].split("_");

                    c.shift();
                    d.shift();

                    if (c.join("_") < d.join("_")) return -1;
                    if (c.join("_") > d.join("_")) return 1;
                    else return 0;
                });
                break;

            case "za":
                azSelected = true;
                azReversed = true;
                num.className = "unselected";
                az.className = "selected";
                az.textContent = "Z-A"

                geckos.sort(function (a, b) {
                    c = a["name"].split("_");
                    d = b["name"].split("_");

                    c.shift();
                    d.shift();

                    if (c.join("_") < d.join("_")) return -1;
                    if (c.join("_") > d.join("_")) return 1;
                    else return 0;
                });
                geckos.reverse();
                break;

            default:
                geckos.sort(function (a, b) {
                    if (a["name"] > b["name"]) return 1;
                    if (a["name"] < b["name"]) return -1;
                    else return 0;
                });
                break;
        }

        var noquery = true;

        if (params.get("q") != null) {
            query = params.get("q");
            document.getElementById("search").value = query;
            noquery = false;
        }

        geckoDiv = document.getElementById("geckoDiv");

        for (let i = 0; i < geckos.length; i++) {

            if ((geckos[i]["name"] != '' && geckos[i]["name"] != null)) {

                let name = geckos[i]["name"].split("_")[0] + "." + geckos[i]["name"].split(".")[geckos[i]["name"].split(".").length - 1];

                //constructing bottom div
                let download = document.createElement('a');
                download.href = "https://geckoimages.ddns.net/" + geckos[i]["url"];

                let downloadIcon = document.createElement('img');
                download.title = "download image"
                downloadIcon.src = "assets/download.png";
                downloadIcon.alt = "download";

                download.appendChild(downloadIcon);

                let folder = document.createElement('a');
                folder.href = geckos[i]["driveUrl"];

                let folderIcon = document.createElement('img');
                folder.title = "view in Google Drive"
                folderIcon.src = "assets/folder.png";
                folderIcon.alt = "drive folder";

                folder.appendChild(folderIcon);

                let div = document.createElement('div');
                div.appendChild(download);
                div.appendChild(folder);

                //titles
                let temp = geckos[i]["name"].split("_");

                let title = document.createElement("h3");
                title.innerHTML = temp[0];

                temp.shift();

                let subtitle = document.createElement("p");
                subtitle.innerHTML = temp.join("_");

                //creating main image
                let image = document.createElement('img');
                image.onerror = function () {
                    this.src = "./assets/placeholder.png"
                    this.onerror = null;
                }
                image.title = geckos[i]["name"];
                image.src = "https://geckoimages.ddns.net/" + geckos[i]["url"];
                image.alt = geckos[i]["name"];
                image.loading = "lazy";

                let card = document.createElement('div');
                card.className = "card";

                card.appendChild(title);
                card.appendChild(subtitle);
                card.appendChild(image);
                card.appendChild(div);

                if (!noquery) {
                    if (geckos[i]["name"].toLowerCase().includes(query.toLowerCase().replace(" ", "_"))) {
                        card.style.display = "inline-grid";
                    }
                    else {
                        card.style.display = "none";
                    }
                }

                geckoDiv.appendChild(card);

                geckoList[geckos[i]["name"]] = card;
            }
            else {
                geckos.splice(i--, 1);
            }
        }

    }, "text");

}

var azSelected = false;

var azReversed = false;
function azButton() {
    let az = document.getElementById("az");
    if (!azSelected) {
        sortElements("az");
        azSelected = true;
        az.className = "selected";
        num.className = "unselected";
        numReversed = false;
        num.textContent = "0-9";
    }
    else {
        if (!azReversed) {
            sortElements("za");
            azReversed = true;
            az.textContent = "Z-A"
        }
        else {
            sortElements("az");
            azReversed = false;
            az.textContent = "A-Z"
        }
    }
}

var numReversed = false;
function numButton() {
    let num = document.getElementById("num");
    if (azSelected) {
        sortElements("09");
        azSelected = false;
        az.className = "unselected";
        azReversed = false;
        az.textContent = "A-Z";
        num.className = "selected";
    }
    else {
        if (!numReversed) {
            sortElements("90");
            numReversed = true;
            num.textContent = "9-0";
        }
        else {
            sortElements("09");
            numReversed = false;
            num.textContent = "0-9";
        }
    }
}

function sortElements(method) {
    geckoDiv.innerHTML = "";

    switch (method) {
        case "09":
            geckos.sort(function (a, b) {
                if (a["name"] > b["name"]) return 1;
                if (a["name"] < b["name"]) return -1;
                else return 0;
            });
            params.set("sort", "09");
            break;
        case "90":
            geckos.sort(function (a, b) {
                if (a["name"] > b["name"]) return 1;
                if (a["name"] < b["name"]) return -1;
                else return 0;
            });
            geckos.reverse();
            params.set("sort", "90");
            break;
        case "az":
            geckos.sort(function (a, b) {
                c = a["name"].split("_");
                d = b["name"].split("_");

                c.shift();
                d.shift();

                if (c.join("_") < d.join("_")) return -1;
                if (c.join("_") > d.join("_")) return 1;
                else return 0;
            });
            params.set("sort", "az");
            break;
        case "za":
            geckos.sort(function (a, b) {
                c = a["name"].split("_");
                d = b["name"].split("_");

                c.shift();
                d.shift();

                if (c.join("_") < d.join("_")) return -1;
                if (c.join("_") > d.join("_")) return 1;
                else return 0;
            });
            geckos.reverse();
            params.set("sort", "za");
            break;
    }
    window.history.replaceState({}, '', `${location.urlname}?${params.toString()}`);

    for (let i = 0; i < geckos.length; i++) {
        geckoDiv.appendChild(geckoList[geckos[i]["name"]]);
    }
}
