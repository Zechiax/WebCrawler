import React, { Component } from 'react';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
    static displayName = Layout.name;

    render() {
        return (
            <div className="full-height">
                <NavMenu/>
                <div
                    tag="main"
                    style={{ height: "calc(100% - 75px)" }}
                >
                    {this.props.children}
                </div>
            </div>
        );
    }
}
