document.addEventListener('DOMContentLoaded', function () { Initialrender(); initiate(); });

var query = "";
var currentQuery = "";

function initiate() {
    // Icon Click Focus
    $('div.icon').click(function () {
        $('input#search').focus();
    });

    //Listen for the event
    $("input#search").on("keyup", function (e) {
        // Set Timeout
        clearTimeout($.data(this, 'timer'));

        // Set Search String
        query = $(this).val();
    });

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
        if (geckos[i].toLowerCase().includes(query.toString().toLowerCase().replace(" ", "_"))) {
            geckoList[geckos[i]].style.display = "inline-grid";
        }
        else {
            geckoList[geckos[i]].style.display = "none";
        }
    }
}

function Initialrender() {

    $.get("./geckos/db.txt", function (data) {
        geckos = data.split(" , ");

        geckos.sort();

        geckoDiv = document.getElementById("geckoDiv");

        for (let i = 0; i < geckos.length; i++) {

            if ((geckos[i] != '' && geckos[i] != null)) {

                let name = geckos[i].split("_")[0] + "." + geckos[i].split(".")[geckos[i].split(".").length - 1];

                //constructing bottom div
                let download = document.createElement('a');
                download.href = "./geckos/" + name;

                let downloadIcon = document.createElement('img');
                download.title = "download image"
                downloadIcon.src = "assets/download.png";
                downloadIcon.alt = "download";

                download.appendChild(downloadIcon);

                let folder = document.createElement('img');
                folder.title = "view in Google Drive"
                folder.src = "assets/folder.png";
                folder.alt = "drive folder";

                let div = document.createElement('div');
                div.appendChild(download);
                div.appendChild(folder);

                //titles
                let temp = geckos[i].split("_");

                let title = document.createElement("h3");
                title.innerHTML = temp[0];

                temp.shift();

                let subtitle = document.createElement("p");
                subtitle.innerHTML = temp.join("_");

                //creating main image
                let image = document.createElement('img');
                image.onerror = function () {
                    this.src = "./geckos/placeholder.png"
                    this.onerror = null;
                }
                image.title = geckos[i];
                image.src = "./geckos/" + name;
                image.alt = geckos[i];
                image.loading = "lazy";

                let card = document.createElement('div');
                card.className = "card";

                card.appendChild(title);
                card.appendChild(subtitle);
                card.appendChild(image);
                card.appendChild(div);

                geckoDiv.appendChild(card);

                geckoList[geckos[i]] = card;
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
            geckos.sort();
            break;
        case "90":
            geckos.sort();
            geckos.reverse();
            break;
        case "az":
            geckos.sort(function (a, b) {
                c = a.split("_");
                d = b.split("_");

                c.shift();
                d.shift();

                if (c.join("_") < d.join("_")) return -1;
                if (c.join("_") > d.join("_")) return 1;
                else return 0;
            });
            break;
        case "za":
            geckos.sort(function (a, b) {
                c = a.split("_");
                d = b.split("_");

                c.shift();
                d.shift();

                if (c.join("_") < d.join("_")) return -1;
                if (c.join("_") > d.join("_")) return 1;
                else return 0;
            });
            geckos.reverse();
            break;
    }

    for (let i = 0; i < geckos.length; i++) {
        geckoDiv.appendChild(geckoList[geckos[i]]);
    }
}
