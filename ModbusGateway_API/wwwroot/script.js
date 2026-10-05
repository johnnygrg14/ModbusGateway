//This function shows a given html element using it's id
function show(id) {
    document.getElementById(id).style.display = "block";
}

//This function hides a given html element using it's id
function hide(id) {
    document.getElementById(id).style.display = "none";
}

//This function disables an element using its id
function disableField(id) {
    document.getElementById(id).disabled = true;
}

//This function enables an element using its id
function enableField(id) {
    document.getElementById(id).disabled = false;
}

//Enables all connection fields (used when the client is disconnected)
function enableConnectionFields() {
    enableField("protocol");
    enableField("ipAddress");
    enableField("port");
    enableField("comPort");
    enableField("baudRate");
}

//Enables modbus fields (used when the client is connected)
function enableModbusFields() {
    enableField("function");
    enableField("slaveId");
    enableField("address");
    enableField("value");
    enableField("coilValue");
    enableField("values");
    enableField("count");
    enableField("sendButton");
}

//Disables modbus fields (used when client is disconnected)
function disableModbusFields() {
    disableField("function");
    disableField("slaveId");
    disableField("address");
    disableField("value");
    disableField("coilValue");
    disableField("values");
    disableField("count");
    disableField("sendButton");
}

//Disables connection fields (used when client is disconnected)
function disableConnectionFields() {
    disableField("protocol");
    disableField("ipAddress");
    disableField("port");
    disableField("comPort");
    disableField("baudRate");
}

//This function checks if a client is already connected and handles the UI fields accordingly
async function checkConnectionStatus() {
    const response = await fetch("/connection-status");
    const data = await response.json();

    if (data.connected) {
        disableConnectionFields();
        enableModbusFields();
        disableField("connect");
        enableField("disconnect");
    }
    else {
        enableConnectionFields();
        disableModbusFields();
        enableField("connect");
        disableField("disconnect");
    }
}

//This function shows the right HTML elements depending on the selected protocol
//@todo needs to be deprecated when TCP is removed
function updateProtocolFields() {
    //Note: this is not needed if TCP is deprecated
    //Get the selected protocol from the HTML element
    const protocol = Number(document.getElementById("protocol").value);

    if (protocol === 0) // TCP
    {
        show("ipGroup");
        show("portGroup");

        hide("comGroup");
        hide("baudGroup");
    }
    else // RTU or ASCII
    {
        show("comGroup");
        show("baudGroup");

        hide("ipGroup");
        hide("portGroup");
    }
}

//These instructions listen for changes in the protocol and function select elements
//and call the update fields functions in order to show the right fields
document.getElementById("protocol")
    .addEventListener("change", updateProtocolFields);

document.getElementById("function")
    .addEventListener("change", updateFunctionFields);

/*
*Event listener for the Client Connect button
*Using this button the user tries to establish a connection
*with a modbus client using connection options the user provided
*/
document.getElementById("connect")
    .addEventListener("click", async function () {
        //@todo delete tcp
        //Read the data from the html page
        const protocol = Number(document.getElementById("protocol").value);
        const ipAddress = document.getElementById("ipAddress").value;
        const port = Number(document.getElementById("port").value);

        const comPort = document.getElementById("comPort").value;
        const baudRate = Number(document.getElementById("baudRate").value);

        //Create a connection options object using the data read from above
        let connectionOptions =
        {
            protocol: protocol,
            ipAddress: ipAddress,
            port: port,
            comPort: comPort,
            baudRate: baudRate
        };

        /*
        Send the HTTP request to connect to the Modbus slave
        We use an exception so the program doesnt crash if the connection fails
        */

        try {
            //Send HTTP request
            const response = await fetch("/connect",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(connectionOptions)
            });

            //Check if the connection was successful
            //if not, show the error on the response field and terimante
            //@todo change this so its more use friendly
            if (!response.ok) {
                document.getElementById("response").value =
                    await response.text();
                return;
            }

            //If the code was not terminated before this point it means
            //that the connection was successful and we can show the message
            //and the correct html fields
            document.getElementById("response").value =
                "Connected successfully.";

            disableConnectionFields();
            enableModbusFields();
            enableField("disconnect");
            disableField("connect");
        }
        catch (err) {
            document.getElementById("response").value = err.message;
        }
    });


//Event listener for the disconnect button
//Uses an HTTP request to call the client's disconnect function
//Again we use an exception to make sure an error doesnt terminate
//the program
document.getElementById("disconnect")
    .addEventListener("click", async function ()
    {
        try {
            const response = await fetch("/disconnect",
                {
                    method: "POST"
                });

            if (!response.ok) {
                document.getElementById("response").value =
                    await response.text();
                return;
            }

            document.getElementById("response").value =
                "Disconnected successfully.";

            enableConnectionFields();
            disableModbusFields();
            enableField("connect");
            disableField("disconnect");
        }
        catch (err) {
            document.getElementById("response").value = err.message;
        }
    });

//This function shows the right HTML elements depending on the selected modbus function
function updateFunctionFields()
{
    //Get the function from the function select element
    const selectedFunction = document.getElementById("function").value;

    //Depending on which function is selected we show and hide the corresponding fields 
    if (selectedFunction === "/write-multiple-registers")
    {
        hide("valueGroup");
        hide("countGroup");

        show("valuesGroup");
    }
    else if (selectedFunction === "/write-single-register")
    {
        show("valueGroup");
        show("value");

        hide("coilValue");
        hide("countGroup");
        hide("valuesGroup");
    }
    else if (selectedFunction === "/write-multiple-coils")
    {
        hide("valueGroup");
        hide("countGroup");

        show("valuesGroup");
    }
    else if (selectedFunction === "/write-single-coil")
    {
        hide("valuesGroup");
        hide("countGroup");
        hide("value");

        show("valueGroup");
        show("coilValue");
    }
    else if (selectedFunction === "/read-input-registers")
    {
        hide("valuesGroup");

        hide("valueGroup");
        show("countGroup");
    }
    else if (selectedFunction === "/read-coils")
    {
        hide("valueGroup");
        hide("valuesGroup");

        show("countGroup");
    }

    if (selectedFunction === "/read-holding-registers")
    {
        hide("valueGroup");
        hide("valuesGroup");

        show("countGroup");
    }
    else if (selectedFunction === "/read-discrete-inputs")
    {
        hide("valueGroup");
        hide("valuesGroup");

        show("countGroup");
    }
}

document
    .getElementById("sendButton")
    .addEventListener("click", async function () {
        //Get the selected modbus function and the correct endpoint
        const endpoint = document.getElementById("function").value;

        //Every request needs the following parameters
        const protocol = Number(document.getElementById("protocol").value);
        const ipAddress = document.getElementById("ipAddress").value;
        const port = Number(document.getElementById("port").value);

        const comPort = document.getElementById("comPort").value;
        const baudRate = Number(document.getElementById("baudRate").value);
        //##############################################################

        //Read values from the page
        //Here we use the getInteger function that reads the numerical value of an html element 
        //and it checks if it is in a given range
        //@todo do this for all elements that require it
        const slaveIdInput = getInteger("slaveId", "slaveId", 0, 247); 
        const addressInput = getInteger("address", "Address", 0, 65535);

        let request =
        {
            protocol: protocol,
            ipAddress: ipAddress,
            port: port,

            comPort: comPort,
            baudRate: baudRate,

            SlaveId: slaveIdInput,
            address: addressInput
        };

        //Depending on selected function make sure that the data provided by the user makes sense
        if (endpoint === "/read-coils") {
            request.count = 
                getInteger("count", "Count", 1, 2000);
        }
        else if (endpoint === "/read-discrete-inputs") {
            request.count =
                getInteger("count", "Count", 1, 2000);
        }
        else if (endpoint === "/read-holding-registers") {
            request.count =
                getInteger("count", "Count", 1, 125);
        }
        else if (endpoint === "/read-input-registers") {
            request.count =
                getInteger("count", "Count", 1, 125);
        }
        else if (endpoint === "/write-single-coil") {
            request.value =
                document.getElementById("value").value === "true";
            //We perform the condition above in order to get an actual javascript boolean 
            //and not a string with the words true or false
        }
        else if (endpoint === "/write-single-register") {
            //Read the value from the front page and add it to the request
            request.value =
                getInteger("value", "Value", 0, 65535);
        }
        else if (endpoint === "/write-multiple-coils") {
            //Read the coil values from the front page 
            const valuesInput = document.getElementById("values").value.trim();

            //Check if values contains any data
            if (valuesInput === "") {
                document.getElementById("response").value =
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
                document.getElementById("response").value =
                    "Coil values must be true or false.";

                return;
            }

            //Create a new array of values. This array is of boolean type since we perform
            //a logical operation on each element of the array
            const coilValues = values.map(x => x === "true");

            //Make sure the length of the array is within modbus limits
            if (coilValues.length < 1 || coilValues.length > 1968) {
                document.getElementById("response").value =
                    "You must provide between 1 and 1968 coil values.";

                return;
            }

            //Add the array of values to the request
            request.values = coilValues;
        }
        else if (endpoint === "/write-multiple-registers") {
            //Get the values from the front page using the trim function to ignore whitespace
            const valuesInput = document.getElementById("values").value.trim();

            //Check if values is empty
            if (valuesInput === "") {
                document.getElementById("response").value =
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
                Number(x) <= 65535))
            {
                document.getElementById("response").value =
                    "All register values must be integers between 0 and 65535.";

                return;
            }

            //Create the array of values 
            const registerValues = values.map(x => Number(x));

            //Check if the length of the array is withing modbus spicification limits
            if (registerValues.length < 1 || registerValues.length > 123) {
                document.getElementById("response").value =
                    "You must provide between 1 and 123 register values.";

                return;
            }

            //Add the values to the request
            request.values = registerValues;
        }

        //Send the request to the back end and wait for a response
        //We are using an exception so the program doesnt crash if something unpredictable goes wrong
        try
        {
            //Send HTTP request
            const response = await fetch(endpoint,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(request)
                });


            //Successful write 
            if (response.status === 204) {
                document.getElementById("response").value =
                    "Write completed successfully.";

                return;
            }

            //Something went wrong
            const data = await response.json();

            if (!response.ok) {

                if (data.error === "Modbus Error") {
                    document.getElementById("response").value =
                        `Modbus Error\n` +
                        `Function Code: ${data.functionCode}\n` +
                        `Exception Code: ${data.exceptionCode}\n` +
                        `Message: ${data.message}`;
                }
                else {
                    document.getElementById("response").value =
                        `Request Error\n` +
                        `Message: ${data.message}`;
                }
                

                return;
            }

            //Unexpected successful response containing JSON
            document.getElementById("response").value =
                JSON.stringify(data, null, 2);
        }
        catch (err)
        {
            //If something went wrong with the request we show the
            //exception error message to the user
            //document.getElementById("response").value = err.message;
            document.getElementById("response").value = "Something went wrong.";
            console.log(err.message);
        }
    });

//This event listener is for the Clear Response button and sets the response element
//to an empty string
document
    .getElementById("clearResponse")
    .addEventListener("click", () => {
        document.getElementById("response").value = "";
    });

//We call the update fields functions in order to show the right fields when the page is first shown
updateProtocolFields();
updateFunctionFields();
checkConnectionStatus();

//Reads a number from an html element and 
//returns the number if it is within a given range
//throws error if thats not the case
function getInteger(id, name, min, max)
{
    const input = document.getElementById(id).value.trim();

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
    if (!Number.isInteger(num) || num < min || num > max) {
        return false;
    }

    return true;
}