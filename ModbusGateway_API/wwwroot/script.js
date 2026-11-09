//Object containing the data needed for Modbus operations
const modbusFunctions = {
    READ_COILS: {
        value: "read-coils",
        label: "Read Coils",
        method: "GET",
        path: "/devices",
        resource: "coils"
    },
    READ_DISCRETE_INPUTS: {
        value: "read-discrete-inputs",
        label: "Read Discrete Inputs",
        method: "GET",
        path: "/devices",
        resource: "discrete-inputs"
    },
    READ_HOLDING_REGISTERS: {
        value: "read-holding-registers",
        label: "Read Holding Registers",
        method: "GET",
        path: "/devices",
        resource: "holding-registers"
    },
    READ_INPUT_REGISTERS: {
        value: "read-input-registers",
        label: "Read Input Registers",
        method: "GET",
        path: "/devices",
        resource: "input-registers"
    },
    WRITE_SINGLE_COIL: {
        value: "write-single-coil",
        label: "Write Single Coil",
        method: "PUT",
        path: "/devices",
        resource: "coils",
        writeType: "single"
    },
    WRITE_SINGLE_REGISTER: {
        value: "write-single-register",
        label: "Write Single Register",
        method: "PUT",
        path: "/devices",
        resource: "holding-registers",
        writeType: "single"
    },
    WRITE_MULTIPLE_COILS: {
        value: "write-multiple-coils",
        label: "Write Multiple Coils",
        method: "PUT",
        path: "/devices",
        resource: "coils",
        writeType: "multiple"
    },
    WRITE_MULTIPLE_REGISTERS: {
        value: "write-multiple-registers",
        label: "Write Multiple Registers",
        method: "PUT",
        path: "/devices",
        resource: "holding-registers",
        writeType: "multiple"
    }
};

//Object containing data for the supported serial protocols
const serialProtocols = {
    RTU: {
        value: 0,
        label: "RTU"
    },

    ASCII: {
        value: 1,
        label: "ASCII"
    }
};

const Elements = {
    //Connection options elements
    createConnectionButton:   document.getElementById("createConnection"),
    connectionsList:          document.getElementById("connectionsList"),
    activeConnectionsSection: document.getElementById("activeConnectionsSection"),
    cancelButton:             document.getElementById("cancelButton"),
    createConnectionSection:  document.getElementById("createConnectionSection"),
    protocol:                 document.getElementById("protocol"),
    protocolGroup:            document.getElementById("protocolGroup"),
    comPort:                  document.getElementById("comPort"),
    comGroup:                 document.getElementById("comGroup"),
    baudRate:                 document.getElementById("baudRate"),
    baudGroup:                document.getElementById("baudGroup"),
    connectButton:            document.getElementById("connect"),
    disconnectAllButton:      document.getElementById("disconnectAll"),

    //Modbus Elements
    functionSelect:  document.getElementById("function"),
    deviceId:        document.getElementById("deviceId"),
    address:         document.getElementById("address"),
    count:           document.getElementById("count"),
    value:           document.getElementById("value"),
    coilValue:       document.getElementById("coilValue"),
    values:          document.getElementById("values"),
    validationError: document.getElementById("validationError"),
    sendButton:      document.getElementById("sendButton"),
    valueGroup:      document.getElementById("valueGroup"),
    countGroup:      document.getElementById("countGroup"),
    valuesGroup:     document.getElementById("valuesGroup"),
    deviceGroup:     document.getElementById("deviceGroup"),
    protocolGroup:   document.getElementById("protocolGroup"),
    modbusSection:   document.getElementById("modbusSection"),
    modbusFields:    document.getElementById("modbusFields"),

    //Response section
    responseSection: document.getElementById("responseSection"),
    responseField:   document.getElementById("response"),
    clearResponse:   document.getElementById("clearResponse")
}

let connections = [];
let selectedConnectionId = null;

//###############################################################
//Modbus UI handlers

//Shows the right elements for creating a connection
function showCreateConnectionFields() {
    show(Elements.createConnectionSection);

    hide(Elements.activeConnectionsSection);
    disableModbusFields();
}

//Cancels creating a new connection
function cancelConnectionCreationUI() {
    hide(Elements.createConnectionSection);

    show(Elements.activeConnectionsSection);

    //If a connection is selected allow modbus request
    if (selectedConnectionId === null) {
        disableModbusFields();
    }
    else {
        enableModbusFields();
    }
}

function initializePageUI() {
    //Hide the serial connection configuration elements
    cancelConnectionCreationUI();

    //Initialize Modbus request fields
    updateFunctionFields();
}

function clearResponseField() {
    Elements.responseField.value = "";
}

//Function that shows the right fields based on the selected modbus function:
function updateFunctionFields() {
    //Get the function from the function select element
    const selectedFunction = Elements.functionSelect.value;

    //Depending on which function is selected we show and hide the corresponding fields 
    switch (selectedFunction) {
        case modbusFunctions.WRITE_MULTIPLE_REGISTERS.value:
            hide(Elements.valueGroup);
            hide(Elements.countGroup);

            show(Elements.valuesGroup);
            break;
        case modbusFunctions.WRITE_SINGLE_REGISTER.value:
            show(Elements.valueGroup);
            show(Elements.value);

            hide(Elements.coilValue);
            hide(Elements.countGroup);
            hide(Elements.valuesGroup);
            break;
        case modbusFunctions.WRITE_MULTIPLE_COILS.value:
            hide(Elements.valueGroup);
            hide(Elements.countGroup);

            show(Elements.valuesGroup);
            break;
        case modbusFunctions.WRITE_SINGLE_COIL.value:
            hide(Elements.valuesGroup);
            hide(Elements.countGroup);
            hide(Elements.value);

            show(Elements.valueGroup);
            show(Elements.coilValue);
            break;
        case modbusFunctions.READ_INPUT_REGISTERS.value:
            hide(Elements.valuesGroup);
            hide(Elements.valueGroup);

            show(Elements.countGroup);
            break;
        case modbusFunctions.READ_COILS.value:
            hide(Elements.valueGroup);
            hide(Elements.valuesGroup);

            show(Elements.countGroup);
            break;
        case modbusFunctions.READ_HOLDING_REGISTERS.value:
            hide(Elements.valueGroup);
            hide(Elements.valuesGroup);

            show(Elements.countGroup);
            break;
        case modbusFunctions.READ_DISCRETE_INPUTS.value:
            hide(Elements.valueGroup);
            hide(Elements.valuesGroup);

            show(Elements.countGroup);
            break;
    }
}

//This function disables an element using its id
function disableField(element) {
    element.disabled = true;
}

//This function enables an element using its id
function enableField(element) {
    element.disabled = false;
}

//Adds options to a drop-down menu on the html page 
function addOptionsToSelectElement(selectElement, options) {
    for (const item of Object.values(options)) {

        //Check if the option contains the right fields
        if (item.value === undefined ||
            item.label === undefined) {
            console.error("Invalid option: ", item);
            continue;
        }

        //Create the dropdown element
        const option = document.createElement("option");
        option.value = item.value;
        option.textContent = item.label;

        //Insert it to the select element
        selectElement.appendChild(option);
    }
}

function enableModbusFields() {
    enableField(Elements.modbusFields);
}

function disableModbusFields() { 
    disableField(Elements.modbusFields);
}

function selectConnection(connectionId) {
    //Find the right connection object based on its id
    const connection = connections.find(
        connection => connection.connectionId === connectionId);

    //Check if it is not null and if it is connected
    if (!connection || !connection.isConnected) {
        return;
    }

    //Set it as the selected connection
    selectedConnectionId = connectionId;

    renderConnections(connections);

    enableModbusFields();
}

//Adds a connection to the connections list
function addConnection(connection) {
    //Create the item that will be added to the connection list
    const item = document.createElement("div");
    item.className =
        connection.connectionId === selectedConnectionId
            ? "connection-item selected"
            : "connection-item";

    //Find which protocol the function is using
    const protocolLabel =
        Object.values(serialProtocols)
            .find(protocol => protocol.value === connection.protocol)
            ?.label ?? "Unknown";

    //Create the text for the connection details
    const details = document.createElement("p");
    details.textContent =
        `${protocolLabel} | ` +
        `${connection.comPort} | ` +
        `${connection.baudRate} baud | ` +
        `${connection.isConnected ? "Connected" : "Disconnected"}`;

    //Create the connections select button
    const selectButton = document.createElement("button");
    selectButton.textContent =
        connection.connectionId === selectedConnectionId
            ? "Selected"
            : "Select";

    selectButton.addEventListener("click", () => {
        selectedConnectionId = connection.connectionId;
        loadConnections();
    });

    //Create the connections disconnect button
    const disconnectButton = document.createElement("button");
    disconnectButton.textContent = "Disconnect";

    //Release the connection when the disconnect button is clicked
    disconnectButton.addEventListener("click", async () => {
        await disconnectConnection(connection.connectionId);
    });

    //Append the details and the buttons to the item
    item.append(details, selectButton, disconnectButton);
    //Add the connection to the connection list
    Elements.connectionsList.appendChild(item);
}

async function disconnectConnection(connectionId) {
    //DELETE /connections/{connectionId}
    try {
        //Ask the back end to release a connection 
        const response = await fetch(
            `/connections/${encodeURIComponent(connectionId)}`,
            { method: "DELETE" }
        );

        //Throw an error if it was unsuccessful
        if (!response.ok) {
            throw new Error("Failed to disconnect.");
        }

        //Remove the connection object from the list
        connections = connections.filter(
            connection => connection.connectionId !== connectionId
        );

        //Clear the connection selection
        if (selectedConnectionId === connectionId) {
            selectedConnectionId = null;
            disableModbusFields();
        }

        renderConnections(connections);
    }
    catch (error) {
        alert(error.message);
    }
}

async function createNewConnection() {
    //Create the connection configuration object
    const protocol = Number(Elements.protocol.value);
    const comPort  = Elements.comPort.value;
    const baudRate = Number(Elements.baudRate.value);

    const connectionOptions =
    {
        protocol: protocol,
        comPort:  comPort,
        baudRate: baudRate
    };

    try {
        //Send HTTP request
        const response = await fetch("/connections",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(connectionOptions)
            });

        //Check if the connection was successful
        //if not, show the error on the response field and terimante
        if (!response.ok) {
            const errorMessage = await response.text();

            Elements.responseField.value =
                `Failed to create connection.\n${errorMessage}`;

            return;
        }

        const newConnection = await response.json();

        connections.push({
            ...newConnection,
            isConnected: true
        });

        selectedConnectionId = newConnection.connectionId;

        renderConnections(connections);
        enableModbusFields();

        //Hide connection configuration elements
        cancelConnectionCreationUI();
    }
    catch (error) {
        Elements.responseField.value =
            `Failed to create connection.\n${error.message}`;
    }
}

//Renders a list of connections
function renderConnections(connections) {
    //Clear the list
    Elements.connectionsList.replaceChildren();

    //If no connections exist show a message and disregard the rest of the function
    if (connections.length === 0) {
        Elements.connectionsList.textContent = "No active connections.";
        return;
    }

    //Create each element of the list
    connections.forEach(connection => {
        const item = document.createElement("div");

        //If the id matches the selected connection id then give the 
        //class name "selected" so css makes it green
        item.className =
            connection.connectionId === selectedConnectionId
                ? "connection-item selected"
                : "connection-item";

        const protocol = Object.values(serialProtocols)
            .find(protocol => protocol.value === connection.protocol);

        const details = document.createElement("p");
        details.textContent =
            `${protocol.label} | ` +
            `${connection.comPort} | ` +
            `${connection.baudRate} baud | ` +
            `${connection.isConnected ? "Connected" : "Disconnected"}`;

        //Create a select and a disconnect button for each list entry
        const selectButton = document.createElement("button");
        selectButton.textContent =
            connection.connectionId === selectedConnectionId
                ? "Selected"
                : "Select";

        selectButton.addEventListener("click", () => {
            selectConnection(connection.connectionId);
        });

        const disconnectButton = document.createElement("button");
        disconnectButton.textContent = "Disconnect";

        disconnectButton.addEventListener("click", () => {
            disconnectConnection(connection.connectionId);
        });

        //Add the new connection entry to the list
        item.append(details, selectButton, disconnectButton);
        Elements.connectionsList.appendChild(item);
    });
}

//#################################################################

//Asks the back end for a list of connections 
async function loadConnections() {
    const response = await fetch("/connections");

    if (!response.ok) {
        throw new Error("Failed to load connections.");
    }

    return await response.json();
}

async function disconnectAllConnections() {

    try {
        const response = await fetch("/connections", {
            method: "DELETE"
        });

        if (!response.ok) {
            console.error("failed to disconnect all connections.");
            return;
        }

        connections = [];
        selectedConnectionId = null;

        renderConnections(connections);
        disableModbusFields();
    }
    catch (error) {
        console.error("Failed to disconnect all connections: ", error);
    }
    
}

async function initializeConnections() {
    try {
        connections = await loadConnections();

        renderConnections(connections);

        if (connections.length === 0) {
            disableModbusFields();
        }
    }
    catch (error) {
        console.error("Failed to initialize connections: ", error);
    }
}

//This function shows a given html element using it's id
function show(element) {
    element.hidden = false;
}

//This function hides a given html element using it's id
function hide(element) {
    element.hidden = true;
}

function getRequestData() {
    //Read data from the front end's HTML elements
    const modbusFunction = Elements.functionSelect.value;

    const protocol = Number(Elements.protocol.value);

    const comPort = Elements.comPort.value;
    const baudRate = Number(Elements.baudRate.value);

    const slaveIdInput = getInteger(Elements.deviceId, "slaveId", 0, 247);
    const addressInput = getInteger(Elements.address, "Address", 0, 65535);

    //Create Modbus request object
    let request =
    {
        modbusFunction: modbusFunction,

        protocol: protocol,

        comPort: comPort,
        baudRate: baudRate,

        SlaveId: slaveIdInput,
        address: addressInput
    };

    //Read the rest of the Modbus data from the front end 
    //and make sure it is valid
    if (modbusFunction === modbusFunctions.READ_COILS.value) {
        request.count =
            getInteger(Elements.count, "Count", 1, 2000);
    }
    else if (modbusFunction === modbusFunctions.READ_DISCRETE_INPUTS.value) {
        request.count =
            getInteger(Elements.count, "Count", 1, 2000);
    }
    else if (modbusFunction === modbusFunctions.READ_HOLDING_REGISTERS.value) {
        request.count =
            getInteger(Elements.count, "Count", 1, 125);
    }
    else if (modbusFunction === modbusFunctions.READ_INPUT_REGISTERS.value) {
        request.count =
            getInteger(Elements.count, "Count", 1, 125);
    }
    else if (modbusFunction === modbusFunctions.WRITE_SINGLE_COIL.value) {
        request.value =
            Elements.coilValue.value === "true";
        //We perform the condition above in order to get an actual javascript boolean 
        //and not a string with the words true or false
    }
    else if (modbusFunction === modbusFunctions.WRITE_SINGLE_REGISTER.value) {
        //Read the value from the front page and add it to the request
        request.value =
            getInteger(Elements.value, "Value", 0, 65535);
    }
    else if (modbusFunction === modbusFunctions.WRITE_MULTIPLE_COILS.value) {
        //Read the coil values from the front page 
        const valuesInput = Elements.values.value.trim();

        //Check if values contains any data
        if (valuesInput === "") {
            Elements.responseField.value =
                "Coil values are required.";
            return;
        }

        //Split the data where a comma exists
        const values = valuesInput
            .split(",")
            .map(x => x.trim());

        //Check if every value contains the strings true or false
        //If not return an error and terminate this function
        if (!values.every(x => x === "true" || x === "false")) {
            Elements.responseField.value =
                "Coil values must be true or false.";

            return;
        }

        //Create a new array of values. This array is of boolean type since we perform
        //a logical operation on each element of the array
        const coilValues = values.map(x => x === "true");

        //Make sure the length of the array is within modbus limits
        if (coilValues.length < 1 || coilValues.length > 1968) {
            Elements.responseField.value =
                "You must provide between 1 and 1968 coil values.";

            return;
        }

        //Add the array of values to the request
        request.values = coilValues;
    }
    else if (modbusFunction === modbusFunctions.WRITE_MULTIPLE_REGISTERS.value) {
        //Get the values from the front page using the trim function to ignore whitespace
        const valuesInput = Elements.values.value.trim();

        //Check if values is empty
        if (valuesInput === "") {
            Elements.responseField.value =
                "Register values are required.";
            return;
        }
        //Create a new array by splitting the input values wherever there is a comma
        //and ignoring whitespaces 
        const values = valuesInput
            .split(",")
            .map(x => x.trim());

        //Here we check multiple things for every value in the array
        //Every value must not be empty, be an integer and be between the limits set
        //by the modbus specification
        if (!values.every(x =>
            x !== "" &&
            Number.isInteger(Number(x)) &&
            Number(x) >= 0 &&
            Number(x) <= 65535)) {
            Elements.responseField.value =
                "All register values must be integers between 0 and 65535.";

            return;
        }

        //Create the array of values 
        const registerValues = values.map(x => Number(x));

        //Check if the length of the array is withing modbus spicification limits
        if (registerValues.length < 1 || registerValues.length > 123) {
            Elements.responseField.value =
                "You must provide between 1 and 123 register values.";

            return;
        }

        //Add the values to the request
        request.values = registerValues;
    }

    return request;
}

async function sendModbusRequest() {
    //Hide error label
    Elements.validationError.hidden = true;

    let request;

    try {
        request = getRequestData();
    }
    catch (error) {
        //Show error if reading the data was unsuccessful
        Elements.validationError.textContent = error.message;
        Elements.validationError.hidden = false;
        return;
    }

    //Get the selected modbus operation
    const selectedFunction = Object.values(modbusFunctions).find(
        function (item) {
            return item.value === request.modbusFunction;
        }
    );

    //Create the endpoint name
    let endpoint = `/connections` +
        `/${encodeURIComponent(selectedConnectionId)}` +
        `${selectedFunction.path}` +
        `/${request.SlaveId}` +
        `/${selectedFunction.resource}`;

    //Only write operations need a write type 
    if (selectedFunction.method === "PUT") {
        endpoint += `/${selectedFunction.writeType}`;
    }

    //Add the starting address to the url
    const query = new URLSearchParams({
        address: request.address,
    });

    //Add count to the url
    if (selectedFunction.method === "GET") {
        query.set("count", request.count);
    }

    //Add the query to the endpoint
    endpoint += `?${query}`;

    //Create the request body
    let body;

    if (selectedFunction.writeType === "single") {
        body = JSON.stringify({ value: request.value });
    }
    else if (selectedFunction.writeType === "multiple") {
        body = JSON.stringify({ values: request.values });
    }

    try {
        const response = await fetch(endpoint,
            {
                method: selectedFunction.method,
                headers: {
                    "Content-Type": "application/json"
                },
                body: body
            });

        //Successful write 
        if (response.status === 204) {
            Elements.responseField.value =
                "Write completed successfully.";

            return;
        }

        //Something went wrong
        const data = await response.json();

        if (!response.ok) {

            //Show a Modbus exception
            if (data.error === "Modbus exception") {
                Elements.responseField.value =
                    `Modbus Error\n` +
                    `Function Code: ${data.functionCode}\n` +
                    `Exception Code: ${data.exceptionCode}\n` +
                    `Message: ${data.message}`;
            }
            else {
                //Show other kinds of exceptions
                Elements.responseField.value =
                    `Unexpected error\n` +
                    `Status: ${data.status}\n` +
                    `${data.title}\n` +
                    `${data.detail}`;
            }

            return;
        }

        //Unexpected successful response containing JSON
        Elements.responseField.value =
            JSON.stringify(data, null, 2);
    }
    catch (error) {
        //If something went wrong with the request we show the
        //exception error message to the user
        Elements.responseField.value = "Something went wrong.";
        console.log(error.message);
    }
}

async function initializeConnections() {
    try {
        connections = await loadConnections();

        renderConnections(connections);

        //No connection is selected initially
        selectedConnectionId = null;
        disableModbusFields();
    }
    catch (error) {
        console.error("Failed to load connections: ", error);
    }
}

//Reads a number from an html element and 
//returns the number if it is within a given range
//throws error if thats not the case
function getInteger(element, name, min, max) {
    const input = element.value.trim();

    if (input === "") {
        throw new Error(`${name} is required.`);
    }

    const value = Number(input);

    if (!isIntegerInValidRange(value, min, max)) {
        throw new Error(`${name} must be between ${min} and ${max}`);
    }

    return value;
}

//This function takes in an integer, a minimum and a maximum
//if the number is not a number or is not within the given limits
//it returns false
//otherwise it returns true
function isIntegerInValidRange(num, min, max) {
    if (!Number.isInteger(num) ||
        num < min ||
        num > max)
    {
        return false;
    }

    return true;
}

//These instructions listen for changes in the protocol and function select elements
//and call the update fields functions in order to show the right fields
Elements.functionSelect
    .addEventListener("change", updateFunctionFields);

Elements.connectButton
    .addEventListener("click", createNewConnection);

Elements.createConnectionButton
    .addEventListener("click", showCreateConnectionFields);

Elements.disconnectAllButton
    .addEventListener("click", disconnectAllConnections);

Elements.cancelButton
    .addEventListener("click", cancelConnectionCreationUI);

Elements.sendButton
    .addEventListener("click", sendModbusRequest);

Elements.clearResponse
    .addEventListener("click", clearResponseField);

document.addEventListener("DOMContentLoaded", initializeConnections);

//###############################################################
//Page initialization
addOptionsToSelectElement(Elements.functionSelect, modbusFunctions);
addOptionsToSelectElement(Elements.protocol, serialProtocols);
initializeConnections();
initializePageUI();