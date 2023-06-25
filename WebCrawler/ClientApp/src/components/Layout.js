import React, { Component } from 'react';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
    static displayName = Layout.name;

    render() {
        return (
            <div className="full-height">
                <NavMenu />
                <div className="full-height" tag="main">
                    {this.props.children}
                </div>
            </div>
        );
    }
}
