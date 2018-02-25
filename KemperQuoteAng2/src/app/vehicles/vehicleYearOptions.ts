import { Option } from './Option';

export const vehicleYearOptions = (() => {
    var years = [];
    for (var i = new Date().getFullYear(); i > 1980; i--) {
        years.push(
            { Value: i.toString(), Description: i.toString() }
        );
    }
    return years;
})();