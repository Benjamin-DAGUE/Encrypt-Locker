window.postAddLockerData = (dotNetHelper) => {

    const name = document.getElementById("name").value;
    const password = document.getElementById("password").value;

    window.sha256HashString(password).then(passwordHash => {
        dotNetHelper.invokeMethodAsync('AddLocker', name, passwordHash)
            .catch(error => {
                console.error('Error:', error);
            });
    });
};

window.postOpenLockerData = (dotNetHelper) => {

    const password = document.getElementById("password").value;

    window.sha256HashString(password).then(passwordHash => {
        dotNetHelper.invokeMethodAsync('OpenLocker', passwordHash)
            .catch(error => {
                console.error('Error:', error);
            });
    });
};

window.postDeleteLockerData = (dotNetHelper) => {

    const password = document.getElementById("password").value;

    window.sha256HashString(password).then(passwordHash => {
        dotNetHelper.invokeMethodAsync('DeleteLocker', passwordHash)
            .catch(error => {
                console.error('Error:', error);
            });
    });
};  